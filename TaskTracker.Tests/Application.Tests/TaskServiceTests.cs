using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Services;
using TaskTracker.Application.Validation;
using TaskTracker.Core.Interfaces;
using Models = TaskTracker.Core.Models;
using Task = System.Threading.Tasks.Task;
using ValidationResult = TaskTracker.Application.Validation.ValidationResult;

namespace TaskTracker.Tests.Application.Tests;

[TestFixture]
public class TaskServiceTests
{
    private ITaskRepository _taskRepository;
    private ITaskValidator _taskValidator;
    private ITimeProvider _timeProvider;

    [SetUp]
    public void SetUp()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _taskValidator = Substitute.For<ITaskValidator>();
        _timeProvider = Substitute.For<ITimeProvider>();
    }

    [Test]
    public async Task AddTaskAsync_WithValidDescription_ReturnsSuccessResult()
    {
        // Arrange
        var fixedTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        _timeProvider.UtcNow.Returns(fixedTime);

        _taskValidator.ValidateDescription("Buy groceries").Returns(ValidationResult.Success());

        // Note: mockRepository.AddTask() returns void, so no setup needed

        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.AddTaskAsync("Buy groceries");

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data.Description, Is.EqualTo("Buy groceries"));
            Assert.That(result.Data.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Data.Status, Is.EqualTo(Models.TaskStatus.New));
            Assert.That(result.Data.CreatedAt, Is.EqualTo(fixedTime)); // Now predictable!
            Assert.That(result.Errors, Is.Empty);
        });
    }

    [Test]
    public async Task AddTaskAsync_WithInvalidDescription_ReturnsFailureResult()
    {
        // Arrange
        // Setup validator to return failure
        var validationErrors = new List<string> { "Description cannot be empty" };
        var failureResult = ValidationResult.Failure(validationErrors);
        _taskValidator.ValidateDescription(string.Empty).Returns(failureResult);

        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.AddTaskAsync("");

        // Assert - what should you verify?
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.Errors, Is.EqualTo(validationErrors));
        });
    }

    [Test]
    public async Task AddTaskAsync_WithNullDescription_ReturnsFailure()
    {
        // Arrange
        var validation = ValidationResult.Failure(["Task description is required."]);
        _taskValidator.ValidateDescription(null!).Returns(validation);

        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.AddTaskAsync(null!);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.Errors, Does.Contain("Task description is required."));
        });
    }

    [Test]
    public async Task AddTaskAsync_WithWhitespaceDescription_ReturnsFailure()
    {
        // Arrange
        var validation = ValidationResult.Failure(["Task description is required."]);
        _taskValidator.ValidateDescription(" ").Returns(validation);

        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.AddTaskAsync(" ");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.Errors, Does.Contain("Task description is required."));
        });
    }

    [Test]
    public async Task AddTaskAsync_WithVeryLongDescription_ReturnsSuccess()
    {
        // Arrange
        var longDescription = new string('A', 1000);
        _taskValidator.ValidateDescription(longDescription).Returns(ValidationResult.Success());

        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.AddTaskAsync(longDescription);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data.Description, Is.EqualTo(longDescription));
            Assert.That(result.Errors, Is.Empty);
        });
    }

    [Test]
    public async Task GetAllTasksAsync_WhenRepositoryThrowsJsonException_ReturnsFailureResult()
    {
        // Arrange
        // Setup repository to throw JsonException
        _taskRepository.GetAllTasksAsync().Throws(new JsonException("Malformed JSON"));
        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.GetAllTasksAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.Errors, Does.Contain("Failed to load tasks. Please try again."));
        });
    }

    [Test]
    public async Task GetAllTasksAsync_WhenRepositoryReturnsTasks_ReturnsSuccessResult()
    {
        // Arrange
        var tasks = new List<Models.Task>
        {
            new(Guid.NewGuid(), "Task 1", Models.TaskStatus.New, DateTime.UtcNow),
            new(Guid.NewGuid(), "Task 2", Models.TaskStatus.InProgress, DateTime.UtcNow)
        };
        // Setup repository to return predefined tasks
        _taskRepository.GetAllTasksAsync().Returns(tasks);
        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.GetAllTasksAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.EqualTo(tasks));
            Assert.That(result.Errors, Is.Empty);
        });
    }

    [Test]
    public async Task GetAllTasksAsync_WhenRepositoryReturnsEmptyList_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        _taskRepository.GetAllTasksAsync().Returns([]);
        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.GetAllTasksAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Errors, Is.Empty);
        });
    }

    [Test]
    public async Task UpdateTaskStatusAsync_WithNonExistentTask_ReturnsFailure()
    {
        // Arrange
        var guid = Guid.NewGuid();

        _taskRepository.GetTaskByIdAsync(guid).Returns((Models.Task?)null);

        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.UpdateTaskStatusAsync(guid, Models.TaskStatus.Completed);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.Errors, Does.Contain("Task not found."));
        });
    }

    [Test]
    public async Task UpdateTaskStatusAsync_WithExistingTask_UpdatesAndReturnsTask()
    {
        // Assert

        var fixedTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        _timeProvider.UtcNow.Returns(fixedTime);

        var existingTask = new Models.Task(Guid.NewGuid(), "Buy groceries", Models.TaskStatus.New, fixedTime);

        _taskRepository.GetTaskByIdAsync(Arg.Any<Guid>()).Returns(existingTask);

        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.UpdateTaskStatusAsync(existingTask.Id, Models.TaskStatus.Completed);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data.Description, Is.EqualTo("Buy groceries"));
            Assert.That(result.Data.Status, Is.EqualTo(Models.TaskStatus.Completed));
            Assert.That(result.Data.CreatedAt, Is.EqualTo(fixedTime));
            Assert.That(result.Errors, Is.Empty);
        });
    }

    [Test]
    public async Task UpdateTaskStatusAsync_WhenRepositoryThrowsJsonException_ReturnsFailureResult()
    {
        // Arrange
        var existingTask = new Models.Task(Guid.NewGuid(), "Buy groceries", Models.TaskStatus.New, DateTime.UtcNow);

        _taskRepository.GetTaskByIdAsync(Arg.Any<Guid>()).Returns(existingTask);
        _taskRepository.UpdateTaskAsync(Arg.Any<Models.Task>()).Throws(new JsonException("Malformed JSON"));

        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.UpdateTaskStatusAsync(existingTask.Id, Models.TaskStatus.InProgress);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.Errors, Does.Contain("Failed to update task. Please try again."));
        });
    }

    [Test]
    public async Task UpdateTaskStatusAsync_WithSameStatus_ReturnsSuccess()
    {
        // Arrange
        var task = new Models.Task(Guid.NewGuid(), "Buy groceries", Models.TaskStatus.New, DateTime.UtcNow);
        _taskRepository.GetTaskByIdAsync(task.Id).Returns(task);

        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.UpdateTaskStatusAsync(task.Id, Models.TaskStatus.New);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data.Description, Is.EqualTo("Buy groceries"));
            Assert.That(result.Data.Status, Is.EqualTo(Models.TaskStatus.New));
        });
    }

    [Test]
    public async Task DeleteTaskAsync_WithNonExistentTask_ReturnsFailure()
    {
        // Arrange

        _taskRepository.GetTaskByIdAsync(Arg.Any<Guid>()).Returns((Models.Task?)null);

        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.DeleteTaskAsync(Guid.NewGuid());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.Errors, Does.Contain("Task not found."));
        });
    }

    [Test]
    public async Task DeleteTaskAsync_WithExistingTask_DeletesAndReturnsTask()
    {
        // Arrange
        var fixedTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        _timeProvider.UtcNow.Returns(fixedTime);

        var existingTask = new Models.Task(Guid.NewGuid(), "Buy groceries", Models.TaskStatus.New, fixedTime);

        _taskRepository.GetTaskByIdAsync(Arg.Any<Guid>()).Returns(existingTask);

        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.DeleteTaskAsync(existingTask.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data.Description, Is.EqualTo("Buy groceries"));
            Assert.That(result.Data.CreatedAt, Is.EqualTo(fixedTime));
            Assert.That(result.Errors, Is.Empty);
        });
    }

    [Test]
    public async Task DeleteTaskAsync_WhenRepositoryThrowsJsonException_ReturnsFailureResult()
    {
        // Arrange
        var existingTask = new Models.Task(Guid.NewGuid(), "Buy groceries", Models.TaskStatus.New, DateTime.UtcNow);

        _taskRepository.GetTaskByIdAsync(Arg.Any<Guid>()).Returns(existingTask);
        _taskRepository.DeleteTaskAsync(Arg.Any<Guid>()).Throws(new JsonException("Malformed JSON"));

        var service = new TaskService(_taskRepository, _taskValidator, _timeProvider);

        // Act
        var result = await service.DeleteTaskAsync(existingTask.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.Errors, Does.Contain("Failed to delete task. Please try again."));
        });
    }
}