
namespace Cyival.Build.Configuration;

public interface ITargetLocation
{
    bool IsRemote { get; }
    bool IsResolved { get; }

    PathSolver SourcePathSolver { get; }
    string GlobalSourcePath => SourcePathSolver.GetBasePath();

    /// <summary>
    /// Path defined in manifest, relative.
    /// Not usable each time.
    /// </summary>
    public string? SourcePath { get; }

    void Resolve();
}
