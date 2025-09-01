using NSubstitute;
using TaskTracker.Application.Interfaces;
using TaskTracker.CLI.Commands;
using TaskTracker.CLI.Handlers;
using TaskTracker.CLI.Interfaces;
using TaskTracker.Core.Models;
using Models = TaskTracker.Core.Models;
using Task = System.Threading.Tasks.Task;

namespace TaskTracker.Tests.CLI.Tests
{
    [TestFixture]
    public class AddCommandHandlerTests
    {
        private ITaskService _taskService;
        private IConsoleOutput _output;
        private AddCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _taskService = Substitute.For<ITaskService>();
            _output = Substitute.For<IConsoleOutput>();
            _handler = new AddCommandHandler(_taskService, _output);
        }

        [Test]
        public async Task HandleAsync_WithSuccessfulTaskCreation_ReturnsZeroAndOutputsSuccessMessage()
        {
            // Arrange
            var command = new AddCommand { Description = "Test task" };
            var task = new Models.Task(Guid.NewGuid(), command.Description, Models.TaskStatus.New, DateTime.UtcNow);
            var result = Result<Models.Task>.Success(task);

            _taskService.AddTaskAsync("Test task").Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(0));
            _output.Received().WriteLine(Arg.Is<string>(s =>
                s.Contains("Task added successfully") && s.Contains("Test task")));
        }

        [Test]
        public async Task HandleAsync_WithTaskCreationFailure_ReturnsOneAndOutputsErrorMessage()
        {
            // Arrange
            var command = new AddCommand { Description = "Invalid task" };
            var errors = new List<string> { "Description is required" };
            var result = Result<Models.Task>.Failure(errors);

            _taskService.AddTaskAsync("Invalid task").Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(
                s => s.Contains("Error") && s.Contains("Description is required")));
        }
    }
}
