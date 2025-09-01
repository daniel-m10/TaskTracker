using CommandLine;

namespace TaskTracker.CLI.Commands
{
    [Verb("update", HelpText = "Update task status")]
    public class UpdateCommand
    {
        [Value(0, Required = true, MetaName = "id", HelpText = "Task ID")]
        public Guid Id { get; set; }

        [Option('s', "status", Required = true, HelpText = "New status: New, InProgress, Completed, Cancelled")]
        public string Status { get; set; } = string.Empty;
    }
}
