using System.Text.Json;
using System.Text;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace TaskManagementSystem
{
    public class RabbitMQHandler
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _queueName;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQHandler(string hostName, string userName, string password, string queueName)
        {
            _hostName = hostName;
            _userName = userName;
            _password = password;
            _queueName = queueName;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public virtual void SendMessage(object obj)
        {
            var messageBody = JsonSerializer.Serialize(obj);
            var body = Encoding.UTF8.GetBytes(messageBody);

            _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
        }

        public void ReceiveMessage(Func<Task, Task> onMessageReceived)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var taskObj = JsonSerializer.Deserialize<Task>(message);

                if (taskObj != null)
                {
                    await onMessageReceived(taskObj);
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
