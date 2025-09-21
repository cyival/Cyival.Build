namespace Cyival.Build.Plugin.Default;

using Build;
using Configuration;
using Environment;

[Plugin("cyival.build")]
public class DefaultPlugin : Plugin
{
    public override void Initialize(PluginStore store)
    {
        store.RegisterConfigurationProvider<GodotConfigurationProvider>("godot");
        store.RegisterEnvironmentProvider<GodotSysProvider>("godot.system");
        store.RegisterEnvironmentProvider<GodotEnvProvider>("godot.godotenv");
        store.RegisterTargetBuilder<GodotTargetBuilder>("godot");
        store.RegisterTargetType<GodotTarget>("godot");
        
        store.RegisterConfigurationProvider<CopyOnlyConfigurationProvider>("copy");
        store.RegisterTargetBuilder<CopyOnlyTargetBuilder>("copy");
        store.RegisterTargetType<CopyOnlyTarget>("copy");
    }
}