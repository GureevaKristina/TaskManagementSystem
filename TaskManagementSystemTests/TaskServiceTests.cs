using Moq;
using TaskManagementSystem.Database;
using TaskManagementSystem.Services;
using TaskManagementSystem;
using Castle.Core.Configuration;

namespace TaskManagementSystemTests
{
    public class TaskServiceTests
    {
        [Fact]
        public async Task AddTask_WithValidData_ShouldAddTaskAndSendMessage()
        {
            // Arrange
            var mockRepo = new Mock<ITaskRepository>();
            var mockBus = new Mock<RabbitMQHandler>("localhost", "guest", "guest", "testQueue");
            var taskService = new TaskService(mockRepo.Object, mockBus.Object);

            var task = new TaskManagementSystem.Models.Task { ID = 1, Name = "Test Task", Description = "Test Description", Status = TaskManagementSystem.Models.TaskStatus.NotStarted };

            // Act
            await taskService.AddTaskAsync(task);

            // Assert
            mockRepo.Verify(r => r.AddTaskAsync(task), Times.Once);
            mockBus.Verify(b => b.SendMessage(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task AddTask_MissingName_ShouldNotAddTaskAndSendMessage()
        {
            // Arrange
            var mockRepo = new Mock<ITaskRepository>(); 
            var mockRabbitMQHandler = new Mock<RabbitMQHandler>("localhost", "guest", "guest", "testQueue");

            var taskService = new TaskService(mockRepo.Object, mockRabbitMQHandler.Object);

            var task = new TaskManagementSystem.Models.Task { ID = 1, Description = "Test Description", Status = TaskManagementSystem.Models.TaskStatus.NotStarted };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await taskService.AddTaskAsync(task));

            // Verify no calls were made
            mockRepo.Verify(r => r.AddTaskAsync(It.IsAny<TaskManagementSystem.Models.Task>()), Times.Never);
            mockRabbitMQHandler.Verify(b => b.SendMessage(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTaskStatus_WithValidId_ShouldUpdateStatusAndSendMessage()
        {
            // Arrange
            var mockRepo = new Mock<ITaskRepository>();
            var mockBus = new Mock<RabbitMQHandler>("localhost", "guest", "guest", "testQueue");
            var taskService = new TaskService(mockRepo.Object, mockBus.Object);

            int taskId = 1;
            var newStatus = TaskManagementSystem.Models.TaskStatus.InProgress;

            // Act
            await taskService.UpdateTaskStatusAsync(taskId, newStatus);

            // Assert
            mockRepo.Verify(r => r.UpdateTaskStatusAsync(taskId, newStatus), Times.Once);
            mockBus.Verify(b => b.SendMessage(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskStatus_InvalidId_ShouldNotUpdateStatusOrSendMessage()
        {
            // Arrange
            var mockRepo = new Mock<ITaskRepository>();
            var mockBus = new Mock<RabbitMQHandler>("localhost", "guest", "guest", "testQueue");
            var taskService = new TaskService(mockRepo.Object, mockBus.Object);

            int invalidTaskId = 999;
            var newStatus = TaskManagementSystem.Models.TaskStatus.InProgress;

            mockRepo.Setup(r => r.UpdateTaskStatusAsync(invalidTaskId, newStatus)).Throws(new KeyNotFoundException());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await taskService.UpdateTaskStatusAsync(invalidTaskId, newStatus));

            // Verify no message was sent
            mockBus.Verify(b => b.SendMessage(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task GetTasks_ShouldReturnListOfTasks()
        {
            // Arrange
            var mockRepo = new Mock<ITaskRepository>();
            var mockBus = new Mock<RabbitMQHandler>("localhost", "guest", "guest", "testQueue");
            var taskService = new TaskService(mockRepo.Object, mockBus.Object);

            var taskList = new List<TaskManagementSystem.Models.Task>
                {
                    new TaskManagementSystem.Models.Task { ID = 1, Name = "Task 1", Status = TaskManagementSystem.Models.TaskStatus.NotStarted },
                    new TaskManagementSystem.Models.Task { ID = 2, Name = "Task 2", Status = TaskManagementSystem.Models.TaskStatus.Completed }
                };

            mockRepo.Setup(r => r.GetTasksAsync()).ReturnsAsync(taskList);

            // Act
            var result = await taskService.GetTasksAsync();

            // Assert
            Assert.Equal(taskList, result);
            mockRepo.Verify(r => r.GetTasksAsync(), Times.Once);
        }
        [Fact]
        public async Task AddTask_Exception_ShouldNotSendMessage()
        {
            // Arrange
            var mockRepo = new Mock<ITaskRepository>();
            var mockRabbitMQHandler = new Mock<RabbitMQHandler>("localhost", "guest", "guest", "testQueue");
            var taskService = new TaskService(mockRepo.Object, mockRabbitMQHandler.Object);

            var task = new TaskManagementSystem.Models.Task { ID = 1, Name = "Task with Error", Description = "Test Description", Status = TaskManagementSystem.Models.TaskStatus.NotStarted };

            mockRepo.Setup(r => r.AddTaskAsync(task)).Throws(new Exception("Database Error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => await taskService.AddTaskAsync(task));
            mockRabbitMQHandler.Verify(m => m.SendMessage(It.IsAny<object>()), Times.Never);
        }
    }
}