using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models
{
    public class Task
    {
        [Required]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public TaskStatus Status { get; set; }
        public string? AssignedTo { get; set; }
    }
}
