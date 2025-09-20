using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Cyival.Build.Cli.Utils;

public class AnsiConsoleLogger(string name) : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var level = Enum.GetName(logLevel)?[..3]?.ToUpper() ?? "";
        
        AnsiConsole.WriteLine($"[{name}] {level,-4}: {formatter(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;
}

[ProviderAlias("AnsiConsole")]
public sealed class AnsiConsoleLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, AnsiConsoleLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new AnsiConsoleLogger(name));

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public static class AnsiConsoleLoggerExtensions
{
    public static ILoggingBuilder AddAnsiConsole(
        this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, AnsiConsoleLoggerProvider>());

        return builder;
    }
}