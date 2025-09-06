using Cyival.Build.Configuration;

namespace Cyival.Build.Build;

public class GodotTarget : TargetBase, IBuildTarget
{
    private GodotConfiguration? _localConfiguration;
    
    public void SetLocalConfiguration<T>(T configuration)
    {
        if (configuration is GodotConfiguration godotConfiguration)
            _localConfiguration = godotConfiguration;
    }

    public T? GetLocalConfiguration<T>()
    {
        if (typeof(T) == typeof(GodotConfiguration) && _localConfiguration.HasValue)
            // I don't know what the fuck is this type casting means.
            return (T)(object)_localConfiguration;

        return default;
    }

    public GodotTarget(string path, string id, IEnumerable<string>? requirements = null)
        : base(path, id, requirements)
    {
        
    }
}