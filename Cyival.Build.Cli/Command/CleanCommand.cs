using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Cyival.Build.Cli.Command;

public sealed class CleanCommand : Command<CleanCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[PATH]")]
        [DefaultValue(".")]
        public required string Path { get; set; }

        [CommandOption("-y")]
        [Description("Delete EVERYTHING without confirmation.")]
        public bool AgreeAll { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var tempDirectories = Directory.GetDirectories(settings.Path, ".cybuild", SearchOption.AllDirectories);
        var directories = tempDirectories.Select(dir => Path.GetFullPath(Path.Combine(dir, "..")));

        foreach (var dir in directories)
        {
            var confirmation = true;
            
            if (!settings.AgreeAll)
                confirmation = AnsiConsole.Prompt(
                    new ConfirmationPrompt($"Delete {dir}?"));
            
            if (confirmation)
                Directory.Delete(dir, true);
        }
        
        return 0;
    }
}