namespace Cyival.Build.Build;

public interface IBuildTarget
{
    public string Path { get; }
    public string Id { get; }
    public List<string> Requirements { get; }
}