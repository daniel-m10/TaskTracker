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
    public class UpdateCommandHandlerTests
    {
        private ITaskService _taskService;
        private IConsoleOutput _output;
        private UpdateCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _taskService = Substitute.For<ITaskService>();
            _output = Substitute.For<IConsoleOutput>();
            _handler = new UpdateCommandHandler(_taskService, _output);
        }

        [Test]
        public async Task HandleAsync_WithValidStatusAndSuccessfulUpdate_ReturnsZeroAndOutputsSuccess()
        {
            // Arrange
            var command = new UpdateCommand { Id = Guid.NewGuid(), Status = "InProgress" };
            var updatedTask = new Models.Task(command.Id, "Task 1", Models.TaskStatus.InProgress, DateTime.UtcNow);

            var result = Models.Result<Models.Task>.Success(updatedTask);
            _taskService.UpdateTaskStatusAsync(command.Id, Models.TaskStatus.InProgress).Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(0));
            _output.Received().WriteLine(Arg.Is<string>(
                s =>
                    s.Contains("Task updated successfully") &&
                    s.Contains("Task 1") &&
                    s.Contains("Status: InProgress")
                ));
            _output.DidNotReceive().WriteError(Arg.Any<string>());
        }

        [Test]
        public async Task HandleAsync_WithValidStatusAndUpdateFails_ReturnsOneAndOutputsError()
        {
            // Arrange
            var command = new UpdateCommand { Id = Guid.NewGuid(), Status = "InProgress" };
            var result = Models.Result<Models.Task>.Failure(["There was an error trying to update the task."]);
            _taskService.UpdateTaskStatusAsync(command.Id, Models.TaskStatus.InProgress).Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Contains("There was an error trying to update the task.")));
        }

        [Test]
        public async Task HandleAsync_WithInvalidStatus_ReturnsOneAndOutputsInvalidStatusError()
        {
            // Arrange
            var command = new UpdateCommand { Id = Guid.NewGuid(), Status = "ToDo" };

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Equals($"Invalid status '{command.Status}'. Valid values: New, InProgress, Completed, Cancelled")));
        }

        [Test]
        public async Task HandleAsync_WhenUpdateTaskStatusAsyncThrowsException_ReturnsOneAndOutputsError()
        {
            // Arrange
            var command = new UpdateCommand { Id = Guid.NewGuid(), Status = "InProgress" };
            _taskService.UpdateTaskStatusAsync(command.Id, Models.TaskStatus.InProgress).ThrowsAsync(new JsonException("Unexpected error"));

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Contains("Unexpected error")));
        }

        [Test]
        public async Task HandleAsync_WithStatusInDifferentCase_ReturnsZeroAndOutputsSuccess()
        {
            // Arrange
            var command = new UpdateCommand { Id = Guid.NewGuid(), Status = "cancelled" };
            var updatedTask = new Models.Task(command.Id, "Task 1", Models.TaskStatus.Cancelled, DateTime.UtcNow);

            var result = Models.Result<Models.Task>.Success(updatedTask);
            _taskService.UpdateTaskStatusAsync(command.Id, Models.TaskStatus.Cancelled).Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(0));
            _output.Received().WriteLine(Arg.Is<string>(
            s =>
                s.Contains("Task updated successfully") &&
                s.Contains("Task 1") &&
                s.Contains("Status: Cancelled")
            ));
            _output.DidNotReceive().WriteError(Arg.Any<string>());
        }

        [Test]
        public async Task HandleAsync_WithMultipleErrorMessages_ReturnsOneAndOutputsAllErrors()
        {
            // Arrange
            var command = new UpdateCommand { Id = Guid.NewGuid(), Status = "New" };
            var errors = new List<string> { "Error 1", "Error 2" };
            var result = Models.Result<Models.Task>.Failure(errors);
            _taskService.UpdateTaskStatusAsync(command.Id, Models.TaskStatus.New).Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Contains("Error 1") && s.Contains("Error 2")));
        }

        [Test]
        public async Task HandleAsync_WithNullStatus_ReturnsOneAndOutputsInvalidStatusError()
        {
            // Arrange
            var command = new UpdateCommand { Id = Guid.NewGuid(), Status = null! };

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Equals($"Invalid status '{command.Status}'. Valid values: New, InProgress, Completed, Cancelled")));
        }

        [Test]
        public async Task HandleAsync_WithWhitespaceStatus_ReturnsOneAndOutputsInvalidStatusError()
        {
            // Arrange
            var command = new UpdateCommand { Id = Guid.NewGuid(), Status = " " };

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Equals($"Invalid status '{command.Status}'. Valid values: New, InProgress, Completed, Cancelled")));
        }
    }
}
