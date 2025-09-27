using Cyival.Build.Build;

namespace Cyival.Build.Plugin.Default.Build;

using Configuration;

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

    public bool TryGetLocalConfiguration<T>(out T? configuration)
    {
        if (typeof(T) == typeof(GodotConfiguration) && _localConfiguration.HasValue)
        {
            // I don't know what the fuck is this type casting means.
            configuration = (T)(object)_localConfiguration;

            return true;
        }

        configuration = default;

        return false;
    }

    public GodotTarget(string path, string dest, string id, IEnumerable<string>? requirements = null)
         : base(path, dest, id, requirements)
    {
        
    }
}