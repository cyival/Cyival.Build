namespace Cyival.Build.Plugin.Bundled;

using Configuration;

[Plugin("cyival.build.bundled")]
public class BundledPlugin : Plugin
{
    public override void Initialize(PluginStore store)
    {
        store.RegisterTargetLocationProvider<LocalTargetLocationProvider>("local");
        store.RegisterTargetLocationProvider<GitTargetLocationProvider>("git");
    }
}
