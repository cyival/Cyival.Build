using Cyival.Build.Build;
using Cyival.Build.Configuration;

namespace Cyival.Build.Plugin.Bundled.Configuration;

public class LocalTargetLocation(string srcPath, string globalSrcPath) : ITargetLocation
{
    public bool IsRemote => false;

    public bool IsResolved => true;

    public PathSolver SourcePathSolver => new PathSolver(globalSrcPath);

    public string? SourcePath => srcPath;

    public void Resolve(BuildSettings buildSettings)
    {
        throw new NotImplementedException();
    }
}
