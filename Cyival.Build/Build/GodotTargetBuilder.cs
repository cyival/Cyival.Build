using Cyival.Build.Configuration;
using Cyival.Build.Environment;

namespace Cyival.Build.Build;

public class GodotTargetBuilder : ITargetBuilder<GodotTarget>
{
    private List<GodotInstance> _godotInstances = [];

    private GodotConfiguration _globalGodotConfiguration = new();
    
    public void Build(IBuildTarget target)
    {
        var buildTarget = target as GodotTarget ?? throw new InvalidOperationException("Target is not a GodotTarget");
        
        /*var godotInstance = _godotInstances.FirstOrDefault(i => i.Version == buildTarget.GodotVersion) 
                            ?? _godotInstances.FirstOrDefault(i => i.IsDefault) 
                            ?? throw new InvalidOperationException("No suitable Godot instance found");*/
    }

    public Type[] GetRequiredEnvironmentTypes() => [typeof(GodotInstance)];

    public Type[] GetRequiredConfigurationTypes() => [typeof(GodotConfiguration)];
    
    public void Setup(IEnumerable<object> environment, IEnumerable<object> configuration)
    {
        var instances = environment.OfType<GodotInstance>();
        var godotInstances = instances as GodotInstance[] ?? instances.ToArray();
        if (godotInstances.Length == 0)
            throw new InvalidOperationException("No Godot instances provided");
        
        _godotInstances = godotInstances.OrderBy(t => t).ToList();
        
        var godotConfiguration = configuration.OfType<GodotConfiguration>().ToArray();
        if (godotConfiguration.Length > 0)
            _globalGodotConfiguration = godotConfiguration.First();

    }
}