using System.ComponentModel;
using Cyival.Build.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Cyival.Build.Cli.Command;

using Utils;

public sealed class BuildCommand : Command<BuildCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[PATH]")]
        [DefaultValue("./build.toml")]
        public required string Path { get; set; }
        
        [CommandOption("-o|--out")]
        [DefaultValue("./out")]
        public required string OutPath { get; set; }
    }

    private string _basePath = "";

    private bool _isBuildOkay = true;
    
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
        _basePath = Path.GetDirectoryName(settings.Path) ?? throw new IOException("Failed to get base path from manifest.");
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        ValidatePath(ref settings);
        
        AnsiConsole.MarkupLine($"Building {settings.Path}\n");
        
#if DEBUG
        BuildApp.LoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddAnsiConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
#endif
        
        var stopwatch = Stopwatch.StartNew();
        
        AnsiConsole.Status()
            .Start("Working...", ctx => Build(ctx, settings));
        
        stopwatch.Stop();

        if (_isBuildOkay)
        {
            AnsiConsole.MarkupLine($"\n[green]Build completed[/] in {stopwatch.Elapsed.TotalSeconds:0.##}s.");
            
            return 0;
        }
        
        AnsiConsole.MarkupLine($"\n[red]Build failed[/] in {stopwatch.Elapsed.TotalSeconds:0.##}s.");

        return -1;
        
    }

    private void Build(StatusContext ctx, Settings settings)
    {
        using var app = new BuildApp();
        app.InitializePlugins();
        AnsiConsole.Markup(":check_mark:  Initialized plugins.\n");
                
        var parser = app.CreateManifestParser("godot");
        var manifest = parser.Parse(settings.Path);
                
        //AnsiConsole.MarkupLine(string.Join(' ', manifest.BuildTargets.Select(t => t.Id)));
        //AnsiConsole.MarkupLine(string.Join(' ', manifest.GlobalConfigurations.Select(c => c.GetType().Name)));
                
        AnsiConsole.MarkupLine(":check_mark:  Loaded manifest.");
                
        app.Initialize(manifest);
        AnsiConsole.MarkupLine(":check_mark:  Initialized builder.");
                
        ctx.Status("Checking environment...");
        app.CollectItems();

        ctx.Status("Building...");
        var buildApp = app.Build(null, settings.OutPath);

        while (!buildApp.IsBuildAllDone() && !buildApp.IsAnyError())
        {
            var next = buildApp.GetNext();
                    
            if (next is not {} buildContext)
                break;
                    
            AnsiConsole.MarkupLine($"   Building target: [yellow bold]{buildContext.TargetId}[/]");
            ctx.Status($"Building ... ({buildApp.GetCurrentIndex() + 1} of {buildApp.GetTotalTargets()})");

            buildContext.Build();
        }

        _isBuildOkay = !buildApp.IsAnyError();
    }
}