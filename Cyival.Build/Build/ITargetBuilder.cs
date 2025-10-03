namespace Cyival.Build.Build;

public interface ITargetBuilder<out T> : ITargetBuilderBase
{
    bool ITargetBuilderBase.CanBuild(IBuildTarget target, BuildSettings buildSettings) => target is T;
}