namespace TaskManagementSystem.Database
{
    public interface ITaskRepository
    {
        Task AddTaskAsync(Models.Task task);
        Task UpdateTaskStatusAsync(int taskId, Models.TaskStatus newStatus);
        Task<List<Models.Task>> GetTasksAsync();
    }
}
