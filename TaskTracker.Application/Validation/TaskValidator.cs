namespace TaskTracker.Application.Validation
{
    public class TaskValidator : ITaskValidator
    {
        public ValidationResult ValidateDescription(string description)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(description))
            {
                result.AddError("Task description is required.");
            }

            return result;
        }
    }
}
