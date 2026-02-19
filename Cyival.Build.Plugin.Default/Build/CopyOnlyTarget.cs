using Cyival.Build.Build;
using Cyival.Build.Configuration;

namespace Cyival.Build.Plugin.Default.Build;

public class CopyOnlyTarget : TargetBase, IBuildTarget
{
    public CopyOnlyTarget(ITargetLocation tl, string dest, string id, IEnumerable<string>? requirements = null)
         : base(tl, dest, id, requirements)
    {
    }

    public void SetLocalConfiguration<T>(T configuration)
    {
        throw new NotImplementedException();
    }

    public T? GetLocalConfiguration<T>()
    {
        throw new NotImplementedException();
    }

    public bool TryGetLocalConfiguration<T>(out T? configuration)
    {
        throw new NotImplementedException();
    }
}
