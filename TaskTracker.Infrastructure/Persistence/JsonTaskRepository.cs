using System.Text.Json;
using TaskTracker.Core.Interfaces;
using Models = TaskTracker.Core.Models;

namespace TaskTracker.Infrastructure.Persistence
{
    public class JsonTaskRepository(string? filePath = null) : ITaskRepository
    {
        private readonly string _filePath = filePath ?? GetDefaultFilePath();
        private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public async Task AddTaskAsync(Models.Task task)
        {
            var tasks = await LoadTasksAsync();
            tasks.Add(task);
            await SaveTasksAsync(tasks);
        }

        public async Task DeleteTaskAsync(Guid id)
        {
            var tasks = await LoadTasksAsync();
            tasks.RemoveAll(t => t.Id == id);
            await SaveTasksAsync(tasks);
        }

        public async Task<List<Models.Task>> GetAllTasksAsync()
        {
            return await LoadTasksAsync();
        }

        public async Task<Models.Task?> GetTaskByIdAsync(Guid id)
        {
            var tasks = await LoadTasksAsync();
            return tasks.FirstOrDefault(t => t.Id == id);
        }

        public async Task UpdateTaskAsync(Models.Task task)
        {
            var tasks = await LoadTasksAsync();
            var index = tasks.FindIndex(t => t.Id == task.Id);
            if (index >= 0)
            {
                tasks[index] = task;
                await SaveTasksAsync(tasks);
            }
        }

        private static string GetDefaultFilePath()
        {
            var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var taskTrackerDirectory = Path.Combine(homeDirectory, ".tasktracker");
            Directory.CreateDirectory(taskTrackerDirectory);
            return Path.Combine(taskTrackerDirectory, "tasks.json");
        }

        private async Task<List<Models.Task>> LoadTasksAsync()
        {
            if (!File.Exists(_filePath)) return [];

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                return JsonSerializer.Deserialize<List<Models.Task>>(json) ?? [];
            }
            catch (JsonException)
            {
                // Handle JSON parsing errors (e.g., log the error)
                return [];
            }
        }

        private async Task SaveTasksAsync(List<Models.Task> tasks)
        {
            var json = JsonSerializer.Serialize(tasks, _serializerOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
