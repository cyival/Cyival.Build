using Cyival.Build.Configuration;

namespace Cyival.Build.Build;

/// <summary>
/// A helper class for implementing build targets.
/// This should managed to implement common properties and methods for <see cref="IBuildTarget"/>.
/// </summary>
public abstract class TargetBase
{
    public bool IsDefault { get; set; } = false;

    public string DestinationPath { get; init; }

    public string Id { get; }

    public List<string> Requirements { get; }

    public ITargetLocation TargetLocation { get; }

    public TargetBase(ITargetLocation targetLocation, string dest, string id, IEnumerable<string>? requirements = null)
    {
        TargetLocation = targetLocation;
        DestinationPath = dest;
        Id = id;
        Requirements = requirements?.ToList() ?? [];
    }
}
