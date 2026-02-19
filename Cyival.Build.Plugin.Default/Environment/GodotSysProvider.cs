using Cyival.Build.Environment;

namespace Cyival.Build.Plugin.Default.Environment;

public class GodotSysProvider : IEnvironmentProvider<GodotInstance>
{
    public IEnumerable<GodotInstance> GetEnvironment()
    {
        return [new GodotInstance(System.Environment.GetEnvironmentVariable("GODOT") ?? "")];
    }

    public bool CanProvide()
        => System.Environment.GetEnvironmentVariable("GODOT") is not null;
}