using System.Diagnostics;
using System.Text.Json;
using Cyival.Build.Configuration;
using Cyival.Build.Environment;
using Cyival.Build.Build;
using Cyival.Build.Plugin.Default.Environment;
using Microsoft.Extensions.Logging;

namespace Cyival.Build.Plugin.Default.Build;

using Configuration;

public class GodotTargetBuilder : ITargetBuilder<GodotTarget>
{
    private List<GodotInstance> _godotInstances = [];

    private GodotConfiguration _globalGodotConfiguration;

    private readonly ILogger _logger = BuildApp.LoggerFactory.CreateLogger<GodotTargetBuilder>();
    
    public BuildResult Build(IBuildTarget target, BuildSettings buildSettings)
    {
        if (!buildSettings.IsBuilding(target))
            throw new InvalidOperationException("Invalid settings provided.");
        
        var buildTarget = target as GodotTarget ?? throw new InvalidOperationException("Target is not a GodotTarget");

        var from = buildSettings.GlobalSourcePath;
        var to = buildSettings.GlobalDestinationPath;

        Directory.CreateDirectory(to);
        
        _logger.LogInformation("Detected godot instances: [{}]", string.Join(',', _godotInstances));
        _logger.LogDebug("Global Godot configuration: {}", _globalGodotConfiguration);

        // Get configuration
        GodotConfiguration configuration;
        if (!buildTarget.TryGetLocalConfiguration<GodotConfiguration>(out var localConfiguration))
        {
            _logger.LogInformation("No local godot configuration found, using global configuration");
            configuration = _globalGodotConfiguration;
        }
        else
            configuration = TypeHelper.MergeStructs(_globalGodotConfiguration, localConfiguration);
        
        // Force to use from local configuration
        configuration.IsGodotPack = localConfiguration.IsGodotPack;
        
        _logger.LogDebug("Using godot configuration: {}", configuration);
        
        // Get godot instance for building
        var godotInstance = configuration.SelectMatchOne(_godotInstances)
                            ?? throw new InvalidOperationException("No matched godot instance available");
        
        _logger.LogInformation("Using godot version: {version}; path: {path}", godotInstance.Version, godotInstance.Path);
        
        // Get export preset
        var presets = GetExportPresets(buildSettings, buildTarget, godotInstance);
        var preset = presets.First(p => p.Value == buildSettings.TargetPlatform).Key;
        _logger.LogInformation("Using export preset: {preset} for platform {platform}", preset, buildSettings.TargetPlatform);
        
        // TODO: support custom output file name (read from buildpresets maybe)
        var outFileName = configuration.IsGodotPack ? $"{buildTarget.Id}.pck" : GetOutputFileName(buildSettings.TargetPlatform, buildTarget.Id);

        var exportArgName = configuration.IsGodotPack
            ? "--export-pack"
            : buildSettings.BuildMode switch
            {
                BuildSettings.Mode.Debug => "--export-debug",
                BuildSettings.Mode.Release => "--export-release",
                _ => throw new NotSupportedException(),
            }; 
        
        var startInfo = new ProcessStartInfo(godotInstance.Path, ["--headless", 
            "--path", from, 
            configuration.IsGodotPack ? "--export-pack" : "--export-release", preset, Path.Combine(to, outFileName)])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        _logger.LogInformation("Running command: {filename} {arguments}", startInfo.FileName, string.Join(' ', startInfo.ArgumentList));
        
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start godot process");

        // FIXME: issue #2
        while (!process.StandardOutput.EndOfStream)
        {
            var line = process.StandardOutput.ReadLine();
            if (string.IsNullOrEmpty(line)) continue;
            
            _logger.LogTrace("{}", line);
            BuildApp.ConsoleRedirector.WriteLine(line);
        }

        process.WaitForExit();

        var stderr = process.StandardError.ReadToEnd();
        
        _logger.LogInformation("Godot process exited with code {code}", process.ExitCode);
        _logger.LogInformation("STDOUT: {}", process.StandardOutput.ReadToEnd());
        _logger.LogInformation("STDERR: {}", stderr);

        if (process.ExitCode != 0)
            return BuildResult.Failed;

        if (stderr.Contains("WARNING"))
            return BuildResult.Warning;
        
        return BuildResult.Success;
    }

    private Dictionary<string, BuildSettings.Platform> GetExportPresets(BuildSettings settings, GodotTarget target, GodotInstance instance)
    {
        var presetPath = settings.SourcePathSolver.GetPathTo("export_presets.cfg");

        if (!File.Exists(presetPath))
            throw new FileNotFoundException("Could not find export_presets.cfg", presetPath);

        var rawData = GodotConfigConverter.ConvertByGodotInstance(settings.OutPathSolver, instance, presetPath)
            ?? throw new InvalidDataException("Failed to parse export_presets.cfg");

        var result = new Dictionary<string, BuildSettings.Platform>();
        
        foreach (var kvp in rawData)
        {
            _logger.LogDebug("Parsing preset: {preset}", kvp.Key);
            var preset = ((JsonElement)kvp.Value).Deserialize<Dictionary<string, object>>()
                ?? throw new InvalidDataException("Failed to parse preset data");
            
            if (!preset.ContainsKey("name") || !preset.ContainsKey("platform"))
                continue;

            var name = ((JsonElement)preset["name"]).Deserialize<string>();
            var platform = ((JsonElement)preset["platform"]).Deserialize<string>();
            
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(platform))
                throw new InvalidDataException("Failed to parse preset name or platform");
            
            result.Add(name, ParsePlatform(platform));
        }

        return result;
    }
    
    private static BuildSettings.Platform ParsePlatform(string platformStr) => platformStr switch
    {
        "Windows Desktop" => BuildSettings.Platform.Windows,
        "Linux/X11" => BuildSettings.Platform.Linux,
        "macOS" => BuildSettings.Platform.MacOS,
        _ => throw new NotSupportedException($"Platform {platformStr} is not supported.")
    };

    private static string GetOutputFileName(BuildSettings.Platform platform, string baseName)
    {
        var extension = platform switch
        {
            BuildSettings.Platform.Windows => ".exe",
            BuildSettings.Platform.Linux => ".x86_64",
            BuildSettings.Platform.MacOS => ".app",
            _ => throw new NotSupportedException($"Platform {platform} is not supported.")
        };

        return baseName + extension;
    }
    
    public Type[] GetRequiredEnvironmentTypes() => [typeof(GodotInstance)];

    public Type[] GetRequiredConfigurationTypes() => [typeof(GodotConfiguration)];


    /// <summary>
    /// Set up the builder with specified environment and global configurations.
    /// </summary>
    /// <param name="environment"></param>
    /// <param name="globalConfiguration"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Setup(IEnumerable<object> environment,
        IEnumerable<object> globalConfiguration)
    {
        var instances = environment.OfType<GodotInstance>();
        var godotInstances = instances as GodotInstance[] ?? instances.ToArray();
        if (godotInstances.Length == 0)
            throw new InvalidOperationException("No Godot instances provided");
        
        _godotInstances = godotInstances.OrderBy(t => t.Version).ToList();
        
        var godotConfiguration = globalConfiguration.OfType<GodotConfiguration>().ToArray();
        if (godotConfiguration.Length > 0)
            _globalGodotConfiguration = godotConfiguration.First();
    }
}