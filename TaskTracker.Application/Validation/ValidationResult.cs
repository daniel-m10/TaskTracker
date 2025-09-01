namespace TaskTracker.Application.Validation
{
    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = [];

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public static ValidationResult Success() => new();
        public static ValidationResult Failure(List<string> errors)
        {
            var result = new ValidationResult();
            result.Errors.AddRange(errors);
            return result;
        }
    }
}
