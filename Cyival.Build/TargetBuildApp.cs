﻿using Cyival.Build.Build;
using Cyival.Build.Plugin;

namespace Cyival.Build;

public class TargetBuildApp(IReadOnlyList<IBuildTarget> targets, IEnumerable<ITargetBuilderBase> builders, IEnumerable<object> environments)
{
    private readonly List<IBuildTarget> _targets = targets.ToList();
    private readonly HashSet<ITargetBuilderBase> _builders = builders.ToHashSet();
    private readonly List<object> _environments = environments.ToList();

    private Dictionary<string, BuildResult> _buildResults = [];

    private int _currentIndex = -1;

    public BuildContext? GetNext()
    {
        if (IsBuildAllDone())
            return null;

        // By default, the index starts from -1, so we increment it first
        _currentIndex += 1;
        var target = _targets[_currentIndex];
        
        // Initializes the build context
        // Builders are already filtered in CollectItems, so there must be at least one builder that can build the target
        var context = new BuildContext(this, target, builders.First(b => b.CanBuild(target)))
        {
            TargetType = target.GetType().ToString(), // TODO
        };

        return context;
    }

    public bool IsBuildAllDone() => _currentIndex + 1 >= _targets.Count;

    public int GetCurrentIndex() => _currentIndex;
    public int GetTotalTargets() => targets.Count;

    public bool IsAnyError() => _buildResults.Any(r => r.Value == BuildResult.Failed);
    
    public struct BuildContext(TargetBuildApp app, IBuildTarget target, ITargetBuilderBase targetBuilder)
    {
        public string TargetId => target.Id;
        public IEnumerable<string> Requirements => target.Requirements;
        public string TargetType;

        public void Build()
        {
            if (!Requirements.All(app._buildResults.ContainsKey))
            {
                throw new InvalidOperationException("Cannot build target before its requirements are built.");
            }

            var result = targetBuilder.Build(target);

            if (result == BuildResult.Failed)
                throw new Exception("Failed to build target " + TargetId);

            app._buildResults[TargetId] = result;

            /*app._buildResults[TargetId] = new BuildResult
            {
                Success = true,
                Message = "Build succeeded",
                Artifacts = []
            };*/
        }
    }
}