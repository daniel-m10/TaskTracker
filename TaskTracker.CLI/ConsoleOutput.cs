using TaskTracker.CLI.Interfaces;

namespace TaskTracker.CLI
{
    public class ConsoleOutput : IConsoleOutput
    {
        public void WriteError(string message) => Console.WriteLine(message);

        public void WriteLine(string message) => Console.Error.WriteLine(message);
    }
}
