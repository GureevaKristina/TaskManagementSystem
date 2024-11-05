using System.ComponentModel.DataAnnotations;
using TaskManagementSystem.Database;

namespace TaskManagementSystem.Services
{
    public class TaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly RabbitMQHandler _serviceBusHandler;

        public TaskService(ITaskRepository taskRepository, RabbitMQHandler serviceBusHandler)
        {
            _taskRepository = taskRepository;
            _serviceBusHandler = serviceBusHandler;
        }

        public async Task AddTaskAsync(Models.Task task)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(task);

            if (!Validator.TryValidateObject(task, validationContext, validationResults, validateAllProperties: true))
            {
                var errorMessages = validationResults.Select(vr => vr.ErrorMessage);
                throw new ArgumentException("Invalid task: " + string.Join("; ", errorMessages));
            }

            await _taskRepository.AddTaskAsync(task);
            _serviceBusHandler.SendMessage(new { Action = "TaskAdded", Task = task });
        }

        public async Task UpdateTaskStatusAsync(int taskId, Models.TaskStatus newStatus)
        {
            await _taskRepository.UpdateTaskStatusAsync(taskId, newStatus);
            _serviceBusHandler.SendMessage(new { Action = "TaskUpdated", TaskId = taskId, NewStatus = newStatus });
        }

        public async Task<List<Models.Task>> GetTasksAsync() => await _taskRepository.GetTasksAsync();
    }
}
