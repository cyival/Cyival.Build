using Cyival.Build.Build;
using Cyival.Build.Configuration;
using Cyival.Build.Environment;

namespace Cyival.Build.Plugin;

[Plugin("cyival.build")]
public class DefaultPlugin : Plugin
{
    public override void Initialize(PluginStore store)
    {
        store.RegisterConfigurationProvider("godot", new GodotConfigurationProvider());
        store.RegisterEnvironmentProvider("godot.system", new GodotProvider());
        store.RegisterTargetBuilder("godot", new GodotTargetBuilder());
        store.RegisterTargetType("godot", typeof(GodotTarget));
    }
}