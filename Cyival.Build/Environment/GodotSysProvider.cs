namespace Cyival.Build.Environment;

public class GodotSysProvider : IEnvironmentProvider<GodotInstance>
{
    public IEnumerable<GodotInstance> GetEnvironment()
    {
        // TODO
        return [new GodotInstance()];
    }

    public bool CanProvide()
        => System.Environment.GetEnvironmentVariable("GODOT") is not null;
}