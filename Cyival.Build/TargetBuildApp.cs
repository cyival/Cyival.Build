using Cyival.Build.Build;
using Cyival.Build.Plugin;

namespace Cyival.Build;

public class TargetBuildApp(IReadOnlyList<IBuildTarget> targets, IEnumerable<ITargetBuilderBase> builders, IEnumerable<object> environments)
{
    private readonly List<IBuildTarget> _targets = targets.ToList();
    private readonly HashSet<ITargetBuilderBase> _builders = builders.ToHashSet();
    private readonly List<object> _environments = environments.ToList();

    private int _currentIndex = -1;

    public BuildContext? GetNext()
    {
        if (IsBuildDone())
            return null;

        _currentIndex += 1;
        var target = _targets[_currentIndex];
        
        var context = new BuildContext()
        {
            TargetId = target.Id,
            TargetType = target.GetType().ToString(), // TODO
            Requirements = target.Requirements,
        };

        return context;
    }

    public bool IsBuildDone() => _currentIndex + 1 >= _targets.Count;

    public int GetCurrentIndex() => _currentIndex;
    public int GetTotalTargets() => targets.Count;
    
    public struct BuildContext
    {
        public string TargetId { get; init; }
        public IEnumerable<string> Requirements { get; init; }
        public string TargetType;
    }
}