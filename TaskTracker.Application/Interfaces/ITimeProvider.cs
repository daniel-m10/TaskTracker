namespace TaskTracker.Application.Interfaces
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}
