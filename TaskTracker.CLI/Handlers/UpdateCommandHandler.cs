using System.Text.Json;
using TaskTracker.Application.Interfaces;
using TaskTracker.CLI.Commands;
using TaskTracker.CLI.Interfaces;
using Models = TaskTracker.Core.Models;

namespace TaskTracker.CLI.Handlers
{
    public class UpdateCommandHandler(ITaskService taskService, IConsoleOutput output)
    {
        private readonly ITaskService _taskService = taskService;
        private readonly IConsoleOutput _output = output;

        public async Task<int> HandleAsync(UpdateCommand command)
        {
            try
            {
                if (!Enum.TryParse<Models.TaskStatus>(command.Status, ignoreCase: true, out var status))
                {
                    _output.WriteError($"Invalid status '{command.Status}'. Valid values: New, InProgress, Completed, Cancelled");
                    return 1;
                }

                var result = await _taskService.UpdateTaskStatusAsync(command.Id, status);

                if (result.IsSuccess)
                {
                    _output.WriteLine($"Task updated successfully: {result.Data.Description} - Status: {result.Data.Status}");
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
