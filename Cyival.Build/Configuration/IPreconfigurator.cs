using Cyival.Build.Build;

namespace Cyival.Build.Configuration;

public interface IPreconfigurator
{
    void PreconfigureTargets(ref Dictionary<string, object> targetData);
    void PreconfigureConfigurations(ref BuildManifest manifest);
}