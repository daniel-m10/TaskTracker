using System.Text.Json;
using TaskTracker.Application.Interfaces;
using TaskTracker.CLI.Commands;
using TaskTracker.CLI.Interfaces;

namespace TaskTracker.CLI.Handlers
{
    public class DeleteCommandHandler(ITaskService taskService, IConsoleOutput output)
    {
        private readonly ITaskService _taskService = taskService;
        private readonly IConsoleOutput _output = output;

        public async Task<int> HandleAsync(DeleteCommand command)
        {
            try
            {
                var result = await _taskService.DeleteTaskAsync(command.Id);

                if (result.IsSuccess)
                {
                    _output.WriteLine($"Task deleted successfully: {result.Data.Description}");
                    return 0;
                }

                _output.WriteError($"Error: {string.Join(", ", result.Errors)}");
                return 1;
            }
            catch (JsonException ex)
            {
                _output.WriteError($"Unexpected error: {ex.Message}");
                return 1;
            }
        }
    }
}
