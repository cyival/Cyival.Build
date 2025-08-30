namespace Cyival.Build.Build;

public class GodotTarget : IBuildTarget
{
    public string Path { get; }
    public string Id { get; }
    public List<string> Requirements { get; }

    public GodotTarget(string path, string id, IEnumerable<string>? requirements = null)
    {
        Path = path;
        Id = id;
        Requirements = requirements?.ToList() ?? [];
    }
}