using TaskTracker.Core.Models;
using Models = TaskTracker.Core.Models;

namespace TaskTracker.Application.Interfaces
{
    public interface ITaskService
    {
        Task<Result<Models.Task>> AddTaskAsync(string description);
        Task<Result<List<Models.Task>>> GetAllTasksAsync();
        Task<Result<Models.Task>> UpdateTaskStatusAsync(Guid id, Models.TaskStatus newStatus);
        Task<Result<Models.Task>> DeleteTaskAsync(Guid id);
    }
}
