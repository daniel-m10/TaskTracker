namespace TaskTracker.Core.Models
{
    public class Task(Guid Id, string Description, TaskStatus Status, DateTime CreatedAt)
    {
        public Guid Id { get; } = Id;
        public string Description { get; } = Description;
        public TaskStatus Status { get; set; } = Status;
        public DateTime CreatedAt { get; } = CreatedAt;
    }
}
