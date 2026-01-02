using System.ComponentModel;
using Cyival.Build.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Cyival.Build.Build;
using Karambolo.Extensions.Logging.File;

namespace Cyival.Build.Cli.Command;

using Utils;

[Description("Build a cybuild project.")]
public sealed class BuildCommand : Command<BuildCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[PATH]")]
        [DefaultValue(".")]
        public required string Path { get; set; }

        [CommandOption("-o|--out <PATH>")]
        [DefaultValue("./out")]
        public required string OutPath { get; init; }

        [CommandOption("-t|--target <ID>")]
        public string? TargetId { get; init; }

        [CommandOption("--platform <PLATFORM>")]
        public string? PlatformName { get; init; }

        [CommandOption("--mode <MODE>")]
        [DefaultValue(BuildSettings.Mode.Debug)]
        public BuildSettings.Mode BuildMode { get; init; }

        [CommandOption("--dep-only")]
        public bool DependenciesOnly { get; init; }
    }

    private string _basePath = "";

    private bool _isBuildOkay = true;

    /// <summary>
    /// Validate path and make path absolute.
    /// </summary>
    private void ValidatePath(ref Settings settings)
    {
        if (string.IsNullOrEmpty(settings.Path))
            settings.Path = ".";

        if (Directory.Exists(settings.Path))
        {
            var path = Path.Combine(settings.Path, "Cybuild.toml");
            if (!File.Exists(path))
            {
                path = Path.Combine(settings.Path, "build.toml");
                if (!File.Exists(path))
                    throw new FileNotFoundException($"Manifest not found: {settings.Path}", settings.Path);

                AnsiConsole.MarkupLine("[yellow]WARNING[/]: Instead of \"build.toml\", we are now suggesting to use \"Cybuild.toml\" as filename of the manifest.");
            }

            settings.Path = path;
        }
        else if (!File.Exists(settings.Path))
        {
            throw new FileNotFoundException($"Manifest not found: {settings.Path}", settings.Path);
        }

        settings.Path = Path.GetFullPath(settings.Path);
        _basePath = Path.GetDirectoryName(settings.Path) ?? throw new IOException("Failed to get base path from manifest.");
    }

    private static string GetTempDir(Settings settings) => Path.GetFullPath(
    Path.Combine(settings.Path, "..", settings.OutPath, BuildApp.OutTempDirName)
        );

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var version = typeof(BuildApp).Assembly.GetName().Version;
        AnsiConsole.MarkupLine($"[yellow]Cyival.Build[/] [dim]v{version}[/]\n");

        ValidatePath(ref settings);

        AnsiConsole.MarkupLine($"Building target {settings.TargetId ?? "default"} at {settings.Path}\n");

        // Create the temp dir to ensure the log can be written.
        Directory.CreateDirectory(GetTempDir(settings));

        BuildApp.LoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddFile(o =>
            {
                o.RootPath = GetTempDir(settings);
                o.Files = [new LogFileOptions { Path = "build.log" }];
            });
#if DEBUG
            builder.AddAnsiConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
#endif
        });

        BuildApp.ConsoleRedirector = new AnsiConsoleRedirector();

        var stopwatch = Stopwatch.StartNew();

        AnsiConsole.Status()
            .Start("Working...", ctx => Build(ctx, settings));

        stopwatch.Stop();

        if (_isBuildOkay)
        {
            AnsiConsole.MarkupLine($"[green]Build completed[/] in {stopwatch.Elapsed.TotalSeconds:0.##}s.");

            return 0;
        }

        AnsiConsole.MarkupLine($"[red]Build failed[/] in {stopwatch.Elapsed.TotalSeconds:0.##}s.");

        return -1;

    }

    private void Build(StatusContext ctx, Settings settings)
    {
        var buildSettings = new BuildSettings(settings.OutPath, _basePath)
        {
            TargetArchitecture = RuntimeInformation.OSArchitecture,
            TargetPlatform = BuildSettings.GetCurrentPlatform(),
            BuildMode = settings.BuildMode,
        };

        using var app = new BuildApp(buildSettings);
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
        var buildApp = app.Build(settings.TargetId, settings.OutPath, settings.DependenciesOnly);

        while (!buildApp.IsBuildAllDone() && !buildApp.IsAnyError())
        {
            var next = buildApp.GetNext();

            if (next is not { } buildContext)
                break;

            AnsiConsole.MarkupLine($"   Building target [yellow bold]{buildContext.TargetId}[/]");
            AnsiConsole.WriteLine(); // Use `\n` seems will cause a weird output, so I used `WriteLine()` instead.
            ctx.Status($"Building ... ({buildApp.GetCurrentIndex() + 1} of {buildApp.GetTotalTargets()})");

            buildContext.Build();

            AnsiConsole.MarkupLine($"   Successfully built [yellow bold]{buildContext.TargetId}[/]");
            AnsiConsole.WriteLine();
        }

        _isBuildOkay = !buildApp.IsAnyError();
    }
}
