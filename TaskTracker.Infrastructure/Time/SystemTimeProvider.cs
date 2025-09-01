using TaskTracker.Application.Interfaces;

namespace TaskTracker.Infrastructure.Time
{
    public class SystemTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
