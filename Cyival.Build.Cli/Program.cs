using System.Text;
using Cyival.Build;
using Cyival.Build.Cli.Command;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

var version = typeof(BuildApp).Assembly.GetName().Version;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var app = new CommandApp<RootCommand>();

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
    cfg.AddCommand<BuildCommand>("b");
    cfg.AddCommand<CleanCommand>("clean");
});

return app.Run(args);
