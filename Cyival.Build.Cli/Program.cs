using Cyival.Build;
using Cyival.Build.Cli.Command;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Velopack;
using Spectre.Console.Cli;

VelopackApp.Build().Run();

var version = typeof(BuildApp).Assembly.GetName().Version;

AnsiConsole.MarkupLine($"[yellow]Cyival.Build[/] [dim]v{version}[/]\n");

var app = new CommandApp<BuildCommand>();
app.Configure(cfg => cfg.AddCommand<BuildCommand>("build"));
return app.Run(args);

