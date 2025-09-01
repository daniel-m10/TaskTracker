using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
    public class ListCommandHandlerTests
    {
        private ITaskService _taskService;
        private IConsoleOutput _output;
        private ListCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _taskService = Substitute.For<ITaskService>();
            _output = Substitute.For<IConsoleOutput>();
            _handler = new ListCommandHandler(_taskService, _output);
        }

        [Test]
        public async Task HandleAsync_WithTasks_ReturnsZeroAndOutputsTaskList()
        {
            // Arrange
            var command = new ListCommand();
            var fixedTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

            var tasks = new List<Models.Task>
            {
                new(Guid.NewGuid(), "Task 1", Models.TaskStatus.New, fixedTime),
                new(Guid.NewGuid(), "Task 2", Models.TaskStatus.InProgress, fixedTime)
            };
            var result = Result<List<Models.Task>>.Success(tasks);

            _taskService.GetAllTasksAsync().Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            var calls = _output.ReceivedCalls()
                .Where(c => c.GetMethodInfo().Name == "WriteLine")
                .Select(c => c.GetArguments()[0] as string)
                .ToList();

            _output.DidNotReceive().WriteError(Arg.Any<string>());

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(0));
                Assert.That(calls, Has.Count.EqualTo(3));
            });
            Assert.Multiple(() =>
            {
                Assert.That(calls[0], Is.EqualTo("Tasks:"));
                Assert.That(calls[1], Does.Contain("Task 1"));
                Assert.That(calls[2], Does.Contain("Task 2"));
            });
        }

        [Test]
        public async Task HandleAsync_WithSingleTask_ReturnsZeroAndOutputsSingleTask()
        {
            // Arrange
            var command = new ListCommand();
            var fixedTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

            var tasks = new List<Models.Task>
            {
                new(Guid.NewGuid(), "Solo Task", Models.TaskStatus.New, fixedTime)
            };
            var result = Result<List<Models.Task>>.Success(tasks);

            _taskService.GetAllTasksAsync().Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            var calls = _output.ReceivedCalls()
                .Where(c => c.GetMethodInfo().Name == "WriteLine")
                .Select(c => c.GetArguments()[0] as string)
                .ToList();

            _output.DidNotReceive().WriteError(Arg.Any<string>());

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(0));
                Assert.That(calls, Has.Count.EqualTo(2));
                Assert.That(calls[0], Is.EqualTo("Tasks:"));
                Assert.That(calls[1], Does.Contain("Solo Task"));
            });
        }

        [Test]
        public async Task HandleAsync_WhenTasksCannotBeRead_ReturnsOne()
        {
            // Arrange
            var command = new ListCommand();

            var result = Result<List<Models.Task>>.Failure(["Tasks cannot be read."]);
            _taskService.GetAllTasksAsync().Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Contains("Tasks cannot be read.")));
        }

        [Test]
        public async Task HandleAsync_WhenNoTasksFound_ReturnsZero()
        {
            // Arrange
            var command = new ListCommand();

            var result = Result<List<Models.Task>>.Success([]);
            _taskService.GetAllTasksAsync().Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(0));
            _output.Received().WriteLine(Arg.Is<string>(s => s.Contains("No tasks found.")));
            _output.DidNotReceive().WriteError(Arg.Any<string>());
        }

        [Test]
        public async Task HandleAsync_WhenGetAllTasksAsyncThrowsException_ReturnsOneAndOutputsError()
        {
            // Arrange
            var command = new ListCommand();
            _taskService.GetAllTasksAsync().ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Contains("Unexpected error")));
        }

        [Test]
        public async Task HandleAsync_WithTaskHavingEmptyDescription_ReturnsZeroAndOutputsTaskWithEmptyDescription()
        {
            // Arrange
            var command = new ListCommand();
            var fixedTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var tasks = new List<Models.Task>()
            {
                new(Guid.NewGuid(), string.Empty, Models.TaskStatus.New, fixedTime)
            };

            var result = Result<List<Models.Task>>.Success(tasks);
            _taskService.GetAllTasksAsync().Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            var calls = _output.ReceivedCalls()
                .Where(c => c.GetMethodInfo().Name == "WriteLine")
                .Select(c => c.GetArguments()[0] as string)
                .ToList();
            _output.DidNotReceive().WriteError(Arg.Any<string>());

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(0));
                Assert.That(calls[0], Does.Contain("Tasks:"));
                Assert.That(calls[1], Does.Contain(" - Status: New - Created: 2024-01-01"));
                Assert.That(calls[1], Does.Not.Contain("Solo Task"));
            });
        }

        [Test]
        public async Task HandleAsync_WithTaskHavingLongDescription_ReturnsZeroAndOutputsTaskWithLongDescription()
        {
            // Arrange
            var command = new ListCommand();
            var fixedTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var longDescription = new string('A', 1000);
            var tasks = new List<Models.Task>()
            {
                new(Guid.NewGuid(), longDescription, Models.TaskStatus.New, fixedTime)
            };

            var result = Result<List<Models.Task>>.Success(tasks);
            _taskService.GetAllTasksAsync().Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            var calls = _output.ReceivedCalls()
                .Where(c => c.GetMethodInfo().Name == "WriteLine")
                .Select(c => c.GetArguments()[0] as string)
                .ToList();
            _output.DidNotReceive().WriteError(Arg.Any<string>());

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(0));
                Assert.That(calls[1], Does.Contain(longDescription));
            });
        }

        [Test]
        public async Task HandleAsync_WithTaskOutputsCorrectStatusAndCreatedDate()
        {
            // Arrange
            var command = new ListCommand();
            var fixedTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var tasks = new List<Models.Task>
            {
                new(Guid.NewGuid(), "Test Task", Models.TaskStatus.InProgress, fixedTime)
            };
            var result = Result<List<Models.Task>>.Success(tasks);
            _taskService.GetAllTasksAsync().Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            var calls = _output.ReceivedCalls()
                .Where(c => c.GetMethodInfo().Name == "WriteLine")
                .Select(c => c.GetArguments()[0] as string)
                .ToList();
            _output.DidNotReceive().WriteError(Arg.Any<string>());

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(0));
                Assert.That(calls[1], Does.Contain("Status: InProgress"));
                Assert.That(calls[1], Does.Contain("Created: 2024-01-01"));
            });
        }

        [Test]
        public async Task HandleAsync_WhenTasksCannotBeRead_ReturnsOneAndOutputsAllErrors()
        {
            // Arrange
            var command = new ListCommand();
            var errors = new List<string>() { "Error 1", "Error 2" };
            var result = Result<List<Models.Task>>.Failure(errors);
            _taskService.GetAllTasksAsync().Returns(result);

            // Act
            var exitCode = await _handler.HandleAsync(command);

            // Assert
            Assert.That(exitCode, Is.EqualTo(1));
            _output.Received().WriteError(Arg.Is<string>(s => s.Contains("Error 1") && s.Contains("Error 2")));
        }
    }
}
