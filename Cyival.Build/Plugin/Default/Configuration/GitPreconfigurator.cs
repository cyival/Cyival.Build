using Cyival.Build.Configuration;

namespace Cyival.Build.Plugin.Default.Configuration;

public class GitPreconfigurator : IPreconfigurator
{
    public void PreconfigureTargets(ref Dictionary<string, object> targetData)
    {
        throw new NotImplementedException();
    }

    public void PreconfigureConfigurations(ref BuildManifest manifest)
    {
        throw new NotImplementedException();
    }
}