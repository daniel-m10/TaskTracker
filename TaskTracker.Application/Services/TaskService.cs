using System.Text.Json;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Validation;
using TaskTracker.Core.Interfaces;
using TaskTracker.Core.Models;
using Models = TaskTracker.Core.Models;
using TaskStatus = TaskTracker.Core.Models.TaskStatus;

namespace TaskTracker.Application.Services
{
    public class TaskService(ITaskRepository repository, ITaskValidator validator, ITimeProvider timeProvider) : ITaskService
    {
        private readonly ITaskRepository _repository = repository;
        private readonly ITaskValidator _validator = validator;
        private readonly ITimeProvider _timeProvider = timeProvider;

        public async Task<Result<Models.Task>> AddTaskAsync(string description)
        {
            var validationResult = _validator.ValidateDescription(description);
            if (!validationResult.IsValid)
            {
                return Result<Models.Task>.Failure(validationResult.Errors);
            }

            var task = new Models.Task(Guid.NewGuid(), description, TaskStatus.New, _timeProvider.UtcNow);

            await _repository.AddTaskAsync(task);
            return Result<Models.Task>.Success(task);
        }

        public async Task<Result<List<Models.Task>>> GetAllTasksAsync()
        {
            try
            {
                var tasks = await _repository.GetAllTasksAsync();
                return Result<List<Models.Task>>.Success(tasks);
            }
            catch (JsonException)
            {
                return Result<List<Models.Task>>.Failure(["Failed to load tasks. Please try again."]);
            }
        }

        public async Task<Result<Models.Task>> UpdateTaskStatusAsync(Guid id, TaskStatus newStatus)
        {
            try
            {
                var existingTask = await _repository.GetTaskByIdAsync(id);
                if (existingTask == null)
                {
                    return Result<Models.Task>.Failure(["Task not found."]);
                }

                existingTask.Status = newStatus;
                await _repository.UpdateTaskAsync(existingTask);

                return Result<Models.Task>.Success(existingTask);
            }
            catch (JsonException)
            {
                return Result<Models.Task>.Failure(["Failed to update task. Please try again."]);
            }
        }

        public async Task<Result<Models.Task>> DeleteTaskAsync(Guid id)
        {
            try
            {
                var existingTask = await _repository.GetTaskByIdAsync(id);

                if (existingTask == null)
                {
                    return Result<Models.Task>.Failure(["Task not found."]);
                }

                await _repository.DeleteTaskAsync(id);
                return Result<Models.Task>.Success(existingTask);
            }
            catch (JsonException)
            {
                return Result<Models.Task>.Failure(["Failed to delete task. Please try again."]);
            }
        }
    }
}
