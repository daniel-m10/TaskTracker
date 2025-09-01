using TaskTracker.Application.Interfaces;
using TaskTracker.CLI.Commands;
using TaskTracker.CLI.Interfaces;

namespace TaskTracker.CLI.Handlers
{
    public class ListCommandHandler(ITaskService taskService, IConsoleOutput output)
    {
        private readonly ITaskService _taskService = taskService;
        private readonly IConsoleOutput _output = output;

        public async Task<int> HandleAsync(ListCommand _)
        {
            try
            {
                var result = await _taskService.GetAllTasksAsync();

                if (!result.IsSuccess)
                {
                    _output.WriteError($"Error: {string.Join(", ", result.Errors)}");
                    return 1;
                }

                if (result.Data.Count == 0)
                {
                    _output.WriteLine("No tasks found.");
                    return 0;
                }

                _output.WriteLine("Tasks:");
                foreach (var task in result.Data)
                {
                    _output.WriteLine($"[{task.Id}] {task.Description} - Status: {task.Status} - Created: {task.CreatedAt:yyyy-MM-dd}");
                }
                return 0;
            }
            catch (Exception ex)
            {
                _output.WriteError($"Unexpected error: {ex.Message}");
                return 1;
            }
        }
    }
}
