using Cyival.Build.Build;
using Cyival.Build.Configuration;
using LibGit2Sharp;

namespace Cyival.Build.Plugin.Bundled.Configuration;

public class GitTargetLocation(GitRepository repo) : ITargetLocation
{
    public bool IsRemote => true;

    public bool IsResolved { get; private set; }

    public PathSolver SourcePathSolver { get; private set; } = new(".");

    public string? SourcePath => null;

    private GitRepository _gitRepo = repo;

    public void Resolve(BuildSettings buildSettings)
    {
        SourcePathSolver = buildSettings.OutTempPathSolver.GetSubSolver("git", buildSettings.CurrentBuildingTarget ?? throw new InvalidOperationException());
        var path = SourcePathSolver.GetBasePath();

        var co = new CloneOptions()
        {
            BranchName = _gitRepo.Branch,
        };

        Repository.Clone(_gitRepo.Repository, path, co);
    }
}
