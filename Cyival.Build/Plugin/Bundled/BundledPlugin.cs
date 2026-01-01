namespace Cyival.Build.Plugin.Bundled;

[Plugin("cyival.build.bundled")]
public class BundledPlugin : Plugin
{
    public override void Initialize(PluginStore store)
    {
        store.RegisterTargetLocationProvider<LocalTargetLocationProvider>("local");
    }
}
