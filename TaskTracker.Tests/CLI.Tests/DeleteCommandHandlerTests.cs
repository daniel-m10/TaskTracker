using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;
using TaskTracker.Application.Interfaces;
using TaskTracker.CLI.Commands;
using TaskTracker.CLI.Handlers;
using TaskTracker.CLI.Interfaces;
using Models = TaskTracker.Core.Models;
using Task = System.Threading.Tasks.Task;

namespace TaskTracker.Tests.CLI.Tests
{
    [TestFixture]
    public class DeleteCommandHandlerTests
    {
        private ITaskService _taskService;
        private IConsoleOutput _output;
        private DeleteCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _taskService = Substitute.For<ITaskService>();
            _output = Substitute.For<IConsoleOutput>();
            _handler = new DeleteCommandHandler(_taskService, _output);
        }

        [Test]
        public async Task HandleAsync_WithSuccessfulDelete_ReturnsZeroAndOutputsSuccess()
        {
            // Arrange
            var command = new DeleteCommand { Id = Guid.NewGuid() };
            var deletedTask = new Models.Task(command.Id, "Buy groceries", Models.TaskStatus.Completed, DateTime.UtcNow);
            var result = Models.Result<Models.Task>.Success(deletedTask);
            _taskService.DeleteTaskAsync(command.Id).Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(0));
            _output.Received().WriteLine(Arg.Is<string>(s => s.Contains("Task deleted successfully: Buy groceries")));
            _output.DidNotReceive().WriteError(Arg.Any<string>());
        }

        [Test]
        public async Task HandleAsync_WithDeleteFails_ReturnsOneAndOutputsError()
        {
            // Arrange
            var command = new DeleteCommand { Id = Guid.NewGuid() };
            var result = Models.Result<Models.Task>.Failure(["Error deleting task."]);
            _taskService.DeleteTaskAsync(command.Id).Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Contains("Error deleting task.")));
            _output.DidNotReceive().WriteLine(Arg.Any<string>());
        }

        [Test]
        public async Task HandleAsync_WhenDeleteTaskAsyncThrowsException_ReturnsOneAndOutputsError()
        {
            // Arrange
            var command = new DeleteCommand { Id = Guid.NewGuid() };
            _taskService.DeleteTaskAsync(command.Id).ThrowsAsync(new JsonException("Something bad happened."));

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Contains("Unexpected error: Something bad happened.")));
            _output.DidNotReceive().WriteLine(Arg.Any<string>());
        }

        [Test]
        public async Task HandleAsync_WithDeleteFails_TaskNotFound_ReturnsOneAndOutputsError()
        {
            // Arrange
            var command = new DeleteCommand { Id = Guid.NewGuid() };
            var result = Models.Result<Models.Task>.Failure(["Task not found."]);
            _taskService.DeleteTaskAsync(command.Id).Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Contains("Task not found")));
            _output.DidNotReceive().WriteLine(Arg.Any<string>());
        }

        [Test]
        public async Task HandleAsync_WithDeleteFails_MultipleErrors_ReturnsOneAndOutputsAllErrors()
        {
            // Arrange
            var command = new DeleteCommand { Id = Guid.NewGuid() };
            var errors = new List<string> { "Error 1", "Error 2" };
            var result = Models.Result<Models.Task>.Failure(errors);
            _taskService.DeleteTaskAsync(command.Id).Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Contains("Error 1") && s.Contains("Error 2")));
            _output.DidNotReceive().WriteLine(Arg.Any<string>());
        }

        [Test]
        public async Task HandleAsync_WithNullTaskDescription_ReturnsZeroAndOutputsSuccess()
        {
            // Arrange
            var command = new DeleteCommand { Id = Guid.NewGuid() };
            var task = new Models.Task(command.Id, Description: null!, Models.TaskStatus.InProgress, DateTime.UtcNow);
            var result = Models.Result<Models.Task>.Success(task);
            _taskService.DeleteTaskAsync(command.Id).Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(0));
            _output.Received().WriteLine(Arg.Is<string>(s => s.Contains("Task deleted successfully: ")));
            _output.DidNotReceive().WriteError(Arg.Any<string>());
        }

        [Test]
        public async Task HandleAsync_WithEmptyTaskDescription_ReturnsZeroAndOutputsSuccess()
        {
            // Arrange
            var command = new DeleteCommand { Id = Guid.NewGuid() };
            var task = new Models.Task(command.Id, Description: string.Empty, Models.TaskStatus.InProgress, DateTime.UtcNow);
            var result = Models.Result<Models.Task>.Success(task);
            _taskService.DeleteTaskAsync(command.Id).Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(0));
            _output.Received().WriteLine(Arg.Is<string>(s => s.Contains("Task deleted successfully:")));
            _output.DidNotReceive().WriteError(Arg.Any<string>());
        }

        [Test]
        public async Task HandleAsync_WithVeryLongTaskDescription_ReturnsZeroAndOutputsSuccess()
        {
            // Arrange
            var command = new DeleteCommand { Id = Guid.NewGuid() };
            var longDescription = new string('A', 1000);
            var task = new Models.Task(command.Id, Description: longDescription, Models.TaskStatus.InProgress, DateTime.UtcNow);
            var result = Models.Result<Models.Task>.Success(task);
            _taskService.DeleteTaskAsync(command.Id).Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(0));
            _output.Received().WriteLine(Arg.Is<string>(s => s.Contains(longDescription)));
            _output.DidNotReceive().WriteError(Arg.Any<string>());
        }
    }
}
