using CommandLine;

namespace TaskTracker.CLI.Commands
{
    [Verb("delete", HelpText = "Delete a task")]
    public class DeleteCommand
    {
        [Value(0, Required = true, MetaName = "id", HelpText = "Task ID")]
        public Guid Id { get; set; }
    }
}
