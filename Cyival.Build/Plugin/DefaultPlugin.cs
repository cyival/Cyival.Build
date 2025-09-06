using Cyival.Build.Build;
using Cyival.Build.Configuration;
using Cyival.Build.Environment;

namespace Cyival.Build.Plugin;

[Plugin("cyival.build")]
public class DefaultPlugin : Plugin
{
    public override void Initialize(PluginStore store)
    {
        store.RegisterConfigurationProvider<GodotConfigurationProvider>("godot");
        store.RegisterEnvironmentProvider<GodotSysProvider>("godot.system");
        store.RegisterTargetBuilder<GodotTargetBuilder>("godot");
        store.RegisterTargetType<GodotTarget>("godot");
    }
}