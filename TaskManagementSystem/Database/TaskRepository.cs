using Microsoft.EntityFrameworkCore;

namespace TaskManagementSystem.Database
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskDbContext _context;

        public TaskRepository(TaskDbContext context)
        {
            _context = context;
        }

        public async Task AddTaskAsync(Models.Task task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskStatusAsync(int taskId, Models.TaskStatus newStatus)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task != null)
            {
                task.Status = newStatus;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Models.Task>> GetTasksAsync() => await _context.Tasks.ToListAsync();
    }
}
