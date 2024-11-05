using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TaskController(TaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost]
        public async Task<IActionResult> AddTask([FromBody] Models.Task task)
        {
            await _taskService.AddTaskAsync(task);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] Models.TaskStatus newStatus)
        {
            await _taskService.UpdateTaskStatusAsync(id, newStatus);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _taskService.GetTasksAsync();
            return Ok(tasks);
        }
    }
}
