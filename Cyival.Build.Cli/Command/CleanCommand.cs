using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Cyival.Build.Cli.Command;

[Description("Clean output directories")]
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

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var startPath = Path.GetFullPath(settings.Path);

        // Find a manifest in startPath or its parent (one level up)
        static bool HasManifest(string dir)
            => File.Exists(Path.Combine(dir, "Cybuild.toml")) || File.Exists(Path.Combine(dir, "build.toml"));

        string? manifestDir = null;

        if (Directory.Exists(startPath) && HasManifest(startPath))
            manifestDir = startPath;
        else
        {
            var parent = Path.GetDirectoryName(startPath);
            if (!string.IsNullOrEmpty(parent) && HasManifest(parent))
                manifestDir = parent;
        }

        if (manifestDir is null)
        {
            AnsiConsole.MarkupLine("[yellow]No manifest found in the specified path or its parent. Aborting clean.[/]");
            return 1;
        }

        // Collect .cybuild directories up to depth 2 relative to manifestDir
        var toDeleteParents = new HashSet<string>();

        // Depth 0: manifestDir itself
        var cand = Path.Combine(manifestDir, ".cybuild");
        if (Directory.Exists(cand) && !File.Exists(Path.Combine(cand, ".no_clean")))
            toDeleteParents.Add(Path.GetFullPath(manifestDir));

        // Depth 1 and 2: search immediate children and their immediate children
        foreach (var child in Directory.GetDirectories(manifestDir))
        {
            var childDot = Path.Combine(child, ".cybuild");
            if (Directory.Exists(childDot) && !File.Exists(Path.Combine(childDot, ".no_clean")))
                toDeleteParents.Add(Path.GetFullPath(child));

            foreach (var grand in Directory.GetDirectories(child))
            {
                var grandDot = Path.Combine(grand, ".cybuild");
                if (Directory.Exists(grandDot) && !File.Exists(Path.Combine(grandDot, ".no_clean")))
                    toDeleteParents.Add(Path.GetFullPath(grand));
            }
        }

        if (!toDeleteParents.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No build temporary directories (.cybuild) found within project scope.[/]");
            return 0;
        }

        foreach (var dir in toDeleteParents)
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
