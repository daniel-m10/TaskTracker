namespace TaskTracker.CLI.Interfaces
{
    public interface IConsoleOutput
    {
        void WriteLine(string message);
        void WriteError(string message);
    }
}
