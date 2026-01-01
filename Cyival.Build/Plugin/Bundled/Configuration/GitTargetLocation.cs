using Cyival.Build.Build;
using Cyival.Build.Configuration;
using LibGit2Sharp;

namespace Cyival.Build.Plugin.Bundled.Configuration;

public class GitTargetLocation(Uri url) : ITargetLocation
{
    public bool IsRemote => true;

    public bool IsResolved { get; private set; }

    public PathSolver SourcePathSolver { get; private set; } = null;

    public string? SourcePath => null;

    private Uri _gitRepoUrl = url;

    public void Resolve(BuildSettings buildSettings)
    {
        SourcePathSolver = buildSettings.OutTempPathSolver.GetSubSolver("git", buildSettings.CurrentBuildingTarget ?? throw new InvalidOperationException());
        var path = SourcePathSolver.GetBasePath();

        Repository.Clone(_gitRepoUrl.OriginalString, path);
    }
}
