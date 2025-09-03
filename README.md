
# TaskTracker

TaskTracker is a modular, test-driven CLI application for managing tasks, built with Clean Architecture principles in .NET.

## Features

- Add, update, list, and delete tasks from the command line
- Tasks have descriptions, statuses, and creation dates
- Statuses: New, InProgress, Completed, Cancelled
- Tasks are persisted in a JSON file
- Validation and error handling for all operations
- Comprehensive unit tests and code coverage support

## Project Structure

```sh
TaskTracker.sln                # Solution file
TaskTracker.Core/              # Domain models and interfaces
TaskTracker.Application/       # Business logic, services, validation
TaskTracker.Infrastructure/    # Persistence and time providers
TaskTracker.CLI/               # CLI commands, handlers, and output
TaskTracker.Tests/             # Unit tests for all layers
```

## How to Run

1. Run all unit tests:

   ```sh
   dotnet test
   ```

2. Generate code coverage:

   ```sh
   dotnet test --collect:"XPlat Code Coverage"
   ```

3. View coverage with VS Code Coverage Gutters or ReportGenerator.

## Continuous Integration (CI)

This project uses GitHub Actions to automatically build and test all code on every push and pull request. The workflow is defined in `.github/workflows/ci.yml` and includes:

- Restoring dependencies
- Building the solution
- Running all unit tests
- Collecting code coverage

You can view the status of your builds and tests on the GitHub repository's Actions tab.

## Codespaces & Devcontainer

This project is ready to run in GitHub Codespaces. It includes a `.devcontainer` configuration that sets up a .NET 8 development environment with recommended VS Code extensions. To use:

1. Click the "Code" button in GitHub and select "Open with Codespaces".
2. Your environment will be automatically set up and dependencies restored.
3. You can build, run, and test the app as described above.

## Usage

Below are examples of each available operation:

### Add a Task

Add a new task with a description:

```sh
dotnet run --project TaskTracker.CLI -- add "Buy groceries"
```

### List Tasks

Show all tasks:

```sh
dotnet run --project TaskTracker.CLI -- list
```

### Update Task Status

Update the status of a task (statuses: New, InProgress, Completed, Cancelled):

```sh
dotnet run --project TaskTracker.CLI -- update <taskId> -s Completed
```

Example:

```sh
dotnet run --project TaskTracker.CLI -- update 123e4567-e89b-12d3-a456-426614174000 --status InProgress
```

### Delete a Task

Delete a task by its ID:

```sh
dotnet run --project TaskTracker.CLI -- delete <taskId>
```

Example:

```sh
dotnet run --project TaskTracker.CLI -- delete 123e4567-e89b-12d3-a456-426614174000
```

## How to Test

1. Run all unit tests:

   ```sh
   dotnet test
   ```

2. Generate code coverage:

   ```sh
   dotnet test --collect:"XPlat Code Coverage"
   ```

3. View coverage with VS Code Coverage Gutters or ReportGenerator.

## Extending the Project

- Add new task properties in `TaskTracker.Core/Models/Task.cs`
- Implement new persistence methods in `TaskTracker.Infrastructure/Persistence/`
- Add new CLI commands in `TaskTracker.CLI/Commands/`
- Write tests in `TaskTracker.Tests/`

## Contributing

Pull requests and issues are welcome! Please ensure new code is covered by tests.

## License

MIT
