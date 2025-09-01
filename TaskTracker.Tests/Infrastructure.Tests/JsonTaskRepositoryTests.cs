using TaskTracker.Infrastructure.Persistence;
using Models = TaskTracker.Core.Models;

namespace TaskTracker.Tests.Infrastructure.Tests
{
    [TestFixture]
    public class JsonTaskRepositoryTests
    {
        private string _testFilePath;
        private JsonTaskRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test-tasks-{Guid.NewGuid()}.json");
            _repository = new JsonTaskRepository(_testFilePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testFilePath)) File.Delete(_testFilePath);
        }

        [Test]
        public async Task AddTaskAsync_WithValidTask_SavesTaskToFile()
        {
            // Arrange
            var task = new Models.Task(Guid.NewGuid(), "Test Task", Models.TaskStatus.New, DateTime.UtcNow);

            // Act
            await _repository.AddTaskAsync(task);

            // Assert
            var retrievedTask = await _repository.GetTaskByIdAsync(task.Id);

            {
                Assert.That(retrievedTask, Is.Not.Null);
                Assert.That(retrievedTask.Description, Is.EqualTo("Test Task"));
            }
        }

        [Test]
        public async Task GetAllTasksAsync_WithMultipleTasks_ReturnsAllTasks()
        {
            // Arrange
            var task1 = new Models.Task(Guid.NewGuid(), "Task 1", Models.TaskStatus.New, DateTime.UtcNow);
            var task2 = new Models.Task(Guid.NewGuid(), "Task 2", Models.TaskStatus.New, DateTime.UtcNow);

            await _repository.AddTaskAsync(task1);
            await _repository.AddTaskAsync(task2);

            // Act
            var allTasks = await _repository.GetAllTasksAsync();

            // Assert
            Assert.That(allTasks, Has.Count.EqualTo(2));

            var retrievedTask1 = allTasks.FirstOrDefault(t => t.Id == task1.Id);
            var retrievedTask2 = allTasks.FirstOrDefault(t => t.Id == task2.Id);

            Assert.Multiple(() =>
            {
                Assert.That(retrievedTask1, Is.Not.Null);
                Assert.That(retrievedTask2, Is.Not.Null);
            });

            Assert.Multiple(() =>
            {
                Assert.That(retrievedTask1.Description, Is.EqualTo("Task 1"));
                Assert.That(retrievedTask2.Description, Is.EqualTo("Task 2"));
            });
        }

        [Test]
        public async Task DeleteTaskAsync_WithExistingTask_RemovesTaskFromFile()
        {
            // Arrange
            var task = new Models.Task(Guid.NewGuid(), "Task to delete", Models.TaskStatus.New, DateTime.UtcNow);
            await _repository.AddTaskAsync(task);

            // Act
            await _repository.DeleteTaskAsync(task.Id);

            // Assert
            var retrievedTask = await _repository.GetTaskByIdAsync(task.Id);
            Assert.That(retrievedTask, Is.Null);

            var allTasks = await _repository.GetAllTasksAsync();
            Assert.That(allTasks, Has.Count.EqualTo(0));
        }

        [Test]
        public async Task UpdateTaskAsync_WithExistingTask_UpdatesTaskInFile()
        {
            // Arrange
            var task = new Models.Task(Guid.NewGuid(), "Original Task", Models.TaskStatus.New, DateTime.UtcNow);
            await _repository.AddTaskAsync(task);

            // Act
            var updatedTask = new Models.Task(task.Id, "Updated Task", Models.TaskStatus.Completed, task.CreatedAt);
            await _repository.UpdateTaskAsync(updatedTask);

            // Assert
            var retrievedTask = await _repository.GetTaskByIdAsync(task.Id);
            Assert.That(retrievedTask, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(retrievedTask.Description, Is.EqualTo("Updated Task"));
                Assert.That(retrievedTask.Status, Is.EqualTo(Models.TaskStatus.Completed));
            });
        }
    }
}
