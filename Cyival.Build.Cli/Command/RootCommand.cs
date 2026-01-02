using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Cyival.Build.Cli.Command;

public class RootCommand : Command<RootCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-v|--version")]
        public bool Version { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (settings.Version)
        {
            var version = typeof(BuildApp).Assembly.GetName().Version;
            Console.WriteLine(version);
            return 0;
        }

        AnsiConsole.WriteLine("Use a subcommand or --help for more info.");
        return 0;
    }
}
