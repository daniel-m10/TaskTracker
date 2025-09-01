namespace TaskTracker.Core.Interfaces
{
    public interface ITaskRepository
    {
        Task AddTaskAsync(Models.Task task);
        Task<Models.Task?> GetTaskByIdAsync(Guid id);
        Task<List<Models.Task>> GetAllTasksAsync();
        Task UpdateTaskAsync(Models.Task task);
        Task DeleteTaskAsync(Guid id);
    }
}
