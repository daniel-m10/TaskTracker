using TaskTracker.Application.Interfaces;
using TaskTracker.CLI.Commands;
using TaskTracker.CLI.Interfaces;

namespace TaskTracker.CLI.Handlers
{
    public class AddCommandHandler(ITaskService taskService, IConsoleOutput output)
    {
        private readonly ITaskService _taskService = taskService;
        private readonly IConsoleOutput _output = output;

        public async Task<int> HandleAsync(AddCommand command)
        {
            var result = await _taskService.AddTaskAsync(command.Description);

            if (result.IsSuccess)
            {
                _output.WriteLine($"Task added successfully: {result.Data.Description}");
                return 0;
            }

            _output.WriteError($"Error: {string.Join(", ", result.Errors)}");
            return 1;
        }
    }
}
