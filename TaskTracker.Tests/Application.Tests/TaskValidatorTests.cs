using TaskTracker.Application.Validation;

namespace TaskTracker.Tests.Application.Tests
{
    [TestFixture]
    public class TaskValidatorTests
    {
        private TaskValidator _taskValidator;

        [SetUp]
        public void SetUp()
        {
            _taskValidator = new TaskValidator();
        }

        [Test]
        public void ValidateDescription_WithValidDescription_ReturnsSuccessResult()
        {
            // Arrange && Act
            var result = _taskValidator.ValidateDescription("Buy groceries");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.Errors, Is.Empty);
            });
        }

        [Test]
        public void ValidateDescription_WithNullDescription_ReturnsFailureResult()
        {
            // Arrange && Act
            var result = _taskValidator.ValidateDescription(null!);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors, Does.Contain("Task description is required."));
            });
        }

        [Test]
        public void ValidateDescription_WithSpecialCharacters_ReturnsSuccess()
        {
            // Arrange && Act
            var result = _taskValidator.ValidateDescription("!@#$%^&*()_+=/.<>");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.Errors, Is.Empty);
            });
        }

        [Test]
        public void ValidateDescription_WithVeryLongString_ReturnsSuccess()
        {
            // Arrange
            var longDescription = new string('A', 1000);

            // Act
            var result = _taskValidator.ValidateDescription(longDescription);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.Errors, Is.Empty);
            });
        }

        [TestCase("", "Task description is required.")]
        [TestCase("   ", "Task description is required.")]
        [TestCase("\t\n", "Task description is required.")]
        public void ValidateDescription_WithInvalidDescription_ReturnsFailureResult(string description, string expectedError)
        {
            // Arrange && Act
            var result = _taskValidator.ValidateDescription(description);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors, Does.Contain(expectedError));
            });
        }
    }
}
