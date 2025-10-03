using Cyival.Build;
using Cyival.Build.Cli.Command;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

var version = typeof(BuildApp).Assembly.GetName().Version;

AnsiConsole.MarkupLine($"[yellow]Cyival.Build[/] [dim]v{version}[/]\n");

var app = new CommandApp<BuildCommand>();

#if DEBUG
app.Configure(config =>
{
    config.SetExceptionHandler((ex, resolver) =>
    {
        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        return -99;
    });
});
#endif

app.Configure(cfg =>
{
    cfg.AddCommand<BuildCommand>("build");
    cfg.AddCommand<CleanCommand>("clean");
});

return app.Run(args);