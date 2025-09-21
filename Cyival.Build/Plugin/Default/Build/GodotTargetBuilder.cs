using System.Diagnostics;
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

    private GodotConfiguration _globalGodotConfiguration = new();

    private ILogger _logger = BuildApp.LoggerFactory.CreateLogger<GodotTargetBuilder>();
    
    public BuildResult Build(IBuildTarget target, BuildSettings? buildSettings = null)
    {
        var buildTarget = target as GodotTarget ?? throw new InvalidOperationException("Target is not a GodotTarget");
        
        if (buildSettings is null)
            buildSettings = BuildSettings.GetCurrentBuildSettings();
        
        _logger.LogInformation("Detected godot instances: [{}]", string.Join(',', _godotInstances));
        _logger.LogInformation("Godot configuration: {}", _globalGodotConfiguration);
        
        var godotInstance = _globalGodotConfiguration.SelectMatchOne(_godotInstances)
                            ?? throw new InvalidOperationException("No matched godot instance available");
        
        _logger.LogInformation("Using godot version: {version}; path: {path}", godotInstance.Version, godotInstance.Path);
        
        /* exit code = 1: FAILED */
        
        var startInfo = new ProcessStartInfo(godotInstance.Path, "--version")
        {
            RedirectStandardOutput = true
        };
        using var process = Process.Start(startInfo);
            
        while (!process.StandardOutput.EndOfStream)
            Console.Write(process.StandardOutput.ReadLine());

        return BuildResult.Failed;
    }

    public Type[] GetRequiredEnvironmentTypes() => [typeof(GodotInstance)];

    public Type[] GetRequiredConfigurationTypes() => [typeof(GodotConfiguration)];


    /// <summary>
    /// Set up the builder with specified environment and global configurations.
    /// </summary>
    /// <param name="pathSolver"></param>
    /// <param name="outPath"></param>
    /// <param name="environment"></param>
    /// <param name="globalConfiguration"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Setup(PathSolver pathSolver, string outPath, IEnumerable<object> environment,
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