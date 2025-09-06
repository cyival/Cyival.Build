namespace Cyival.Build.Build;

/// <summary>
/// A helper class for implementing build targets.
/// This should managed to implement common properties and methods for <see cref="IBuildTarget"/>.
/// </summary>
public abstract class TargetBase
{
    public bool IsDefault { get; set; } = false;
    
    public string Path { get; }
    
    public string Id { get; }
    
    public List<string> Requirements { get; }

    public TargetBase(string path, string id, IEnumerable<string>? requirements = null)
    {
        Path = path;
        Id = id;
        Requirements = requirements?.ToList() ?? [];
    }
}