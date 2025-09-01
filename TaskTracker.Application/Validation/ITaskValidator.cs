namespace TaskTracker.Application.Validation
{
    public interface ITaskValidator
    {
        ValidationResult ValidateDescription(string description);
    }
}
