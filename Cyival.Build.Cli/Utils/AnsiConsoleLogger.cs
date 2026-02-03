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

        var level = GetLevelName(logLevel); //Enum.GetName(logLevel)?[..3]?.ToUpper() ?? "";

        // TODO: Give an option for no color
        AnsiConsole.MarkupLine($"[gray][[{name}]][/] [{GetLevelColor(logLevel)}]{level,5}[/]: {formatter(state, exception).EscapeMarkup()}");
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    private string GetLevelColor(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "gray",
            LogLevel.Debug => "gray",
            LogLevel.Information => "green",
            LogLevel.Warning => "yellow",
            LogLevel.Error => "red",
            LogLevel.Critical => "red",
            _ => "gray"
        };
    }

    private string GetLevelName(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRIT",
            _ => Enum.GetName(logLevel) ?? logLevel.ToString(),
        };
    }
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
