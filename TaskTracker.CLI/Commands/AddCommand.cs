using CommandLine;

namespace TaskTracker.CLI.Commands
{
    [Verb("add", HelpText = "Add a new task")]
    public class AddCommand
    {
        [Value(0, Required = true, MetaName = "description", HelpText = "Task description")]
        public string Description { get; set; } = string.Empty;
    }
}
