using System.ComponentModel;
using Cyival.Build.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using Microsoft.Extensions.Logging;

namespace Cyival.Build.Cli.Command;

public sealed class BuildCommand : Command<BuildCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[PATH]")]
        [DefaultValue("./build.toml")]
        public required string Path { get; set; }
    }

    private string BasePath = "";
    
    /// <summary>
    /// Validate path and make path absolute.
    /// </summary>
    private void ValidatePath(ref Settings settings)
    {
        if (Directory.Exists(settings.Path))
        {
            settings.Path = Path.Combine(settings.Path, "build.toml");
        }

        if (!File.Exists(settings.Path))
        {
            throw new FileNotFoundException($"Manifest not found: {settings.Path}", settings.Path);
        }

        settings.Path = Path.GetFullPath(settings.Path);
        BasePath = Path.GetDirectoryName(settings.Path) ?? throw new IOException("Failed to get base path from manifest.");
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        ValidatePath(ref settings);
        
        AnsiConsole.MarkupLine($"Building {settings.Path}\n");
        
#if DEBUG
        //BuildApp.LoggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
#endif

        AnsiConsole.Status()
            .Start("Working...", ctx =>
            {
                using var app = new BuildApp();
                app.InitializePlugins();
                AnsiConsole.Markup(":check_mark:  Initialized plugins.\n");
                
                var parser = app.CreateManifestParser("godot");
                var manifest = parser.Parse(settings.Path);
                
                AnsiConsole.MarkupLine(string.Join(' ', manifest.BuildTargets.Select(t => t.Id)));
                AnsiConsole.MarkupLine(string.Join(' ', manifest.GlobalConfigurations.Select(c => c.GetType().Name)));
                
                AnsiConsole.MarkupLine(":check_mark:  Loaded manifest.");
                
                app.Initialize(manifest);
                AnsiConsole.MarkupLine(":check_mark:  Initialized builder.");
                
                ctx.Status("Checking environment...");
                Thread.Sleep(1000);
                ctx.Status("Building...");
            });
        
        
        throw new NotImplementedException();
    }
}