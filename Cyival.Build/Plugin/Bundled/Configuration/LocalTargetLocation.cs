namespace Cyival.Build.Plugin.Bundled;

using Configuration;

public class LocalTargetLocation(string srcPath, string globalSrcPath) : ITargetLocation
{
    public bool IsRemote => false;

    public bool IsResolved => true;

    public PathSolver SourcePathSolver => new PathSolver(globalSrcPath);

    public string? SourcePath => srcPath;

    public void Resolve()
    {
        throw new NotImplementedException();
    }
}
