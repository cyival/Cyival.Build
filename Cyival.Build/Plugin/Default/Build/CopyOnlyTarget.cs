using Cyival.Build.Build;

namespace Cyival.Build.Plugin.Default.Build;

public class CopyOnlyTarget : TargetBase, IBuildTarget
{
    public CopyOnlyTarget(string path, string dest, string id, IEnumerable<string>? requirements = null)
         : base(path, dest, id, requirements)
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
}