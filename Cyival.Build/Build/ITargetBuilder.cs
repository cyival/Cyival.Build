namespace Cyival.Build.Build;

public interface ITargetBuilder<out T> : ITargetBuilderBase
{
    public virtual bool CanBuild(IBuildTarget target) => target is T;
}