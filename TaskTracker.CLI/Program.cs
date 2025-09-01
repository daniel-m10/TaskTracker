using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Services;
using TaskTracker.Application.Validation;
using TaskTracker.CLI;
using TaskTracker.CLI.Commands;
using TaskTracker.CLI.Handlers;
using TaskTracker.CLI.Interfaces;
using TaskTracker.Core.Interfaces;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.Infrastructure.Time;

var services = new ServiceCollection()
    // Infrastructure
    .AddScoped<ITaskRepository, JsonTaskRepository>()
    .AddScoped<ITimeProvider, SystemTimeProvider>()
    .AddScoped<IConsoleOutput, ConsoleOutput>()

    // Application
    .AddScoped<ITaskValidator, TaskValidator>()
    .AddScoped<ITaskService, TaskService>()

    // Handlers
    .AddScoped<AddCommandHandler>()
    .AddScoped<ListCommandHandler>()
    .AddScoped<UpdateCommandHandler>()
    .AddScoped<DeleteCommandHandler>()
    .BuildServiceProvider();

return await Parser.Default.ParseArguments<AddCommand, ListCommand, UpdateCommand, DeleteCommand>(args)
    .MapResult(
        (AddCommand cmd) => services.GetService<AddCommandHandler>()!.HandleAsync(cmd),
        (ListCommand cmd) => services.GetService<ListCommandHandler>()!.HandleAsync(cmd),
        (UpdateCommand cmd) => services.GetService<UpdateCommandHandler>()!.HandleAsync(cmd),
        (DeleteCommand cmd) => services.GetService<DeleteCommandHandler>()!.HandleAsync(cmd),
    errs => Task.FromResult(1));