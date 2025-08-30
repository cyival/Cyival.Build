namespace Cyival.Build.Environment;

public class GodotProvider : IEnvironmentProvider<GodotInstance>
{
    public IEnumerable<GodotInstance>? GetEnvironment()
    {
        throw new NotImplementedException();
    }

    public bool CanProvide()
        => System.Environment.GetEnvironmentVariable("GODOT") is not null;
}