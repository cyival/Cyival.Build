using Cyival.Build.Configuration;
using Cyival.Build.Environment;
using Cyival.Build.Build;
using Cyival.Build.Plugin.Default.Environment;

namespace Cyival.Build.Plugin.Default.Build;

using Configuration;

public class GodotTargetBuilder : ITargetBuilder<GodotTarget>
{
    private List<GodotInstance> _godotInstances = [];

    private GodotConfiguration _globalGodotConfiguration = new();
    
    public BuildResult Build(IBuildTarget target, BuildSettings? buildSettings = null)
    {
        var buildTarget = target as GodotTarget ?? throw new InvalidOperationException("Target is not a GodotTarget");
        
        if (buildSettings is null)
            buildSettings = BuildSettings.GetCurrentBuildSettings();
        
        /*var godotInstance = _godotInstances.FirstOrDefault(i => i.Version == buildTarget.GodotVersion) 
                            ?? _godotInstances.FirstOrDefault(i => i.IsDefault) 
                            ?? throw new InvalidOperationException("No suitable Godot instance found");*/

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
        
        _godotInstances = godotInstances.OrderBy(t => t).ToList();
        
        var godotConfiguration = globalConfiguration.OfType<GodotConfiguration>().ToArray();
        if (godotConfiguration.Length > 0)
            _globalGodotConfiguration = godotConfiguration.First();
    }
}