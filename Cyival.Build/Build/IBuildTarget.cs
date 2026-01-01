using Cyival.Build.Configuration;

namespace Cyival.Build.Build;

public interface IBuildTarget
{
    public bool IsDefault { get; set; }

    public string DestinationPath { get; init; }

    public string Id { get; }

    public List<string> Requirements { get; }

    public ITargetLocation TargetLocation { get; }

    public void SetLocalConfiguration<T>(T configuration);

    public T? GetLocalConfiguration<T>();

    public bool TryGetLocalConfiguration<T>(out T? configuration);

    /// <summary>
    /// Parse target itself from a dictionary of data.
    /// </summary>
    /// <param name="data"></param>
    public void Parse(Dictionary<string, object> data) { }
}
