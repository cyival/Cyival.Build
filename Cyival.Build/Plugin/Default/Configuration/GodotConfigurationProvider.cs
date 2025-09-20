using Tomlyn.Model;
using Cyival.Build.Configuration;

namespace Cyival.Build.Plugin.Default.Configuration;

public class GodotConfigurationProvider : IConfigurationProvider<GodotConfiguration>
{
    public GodotConfiguration GetDefaultConfiguration()
    {
        // TODO
        return new GodotConfiguration();
    }

    public GodotConfiguration ParseFromTable(TomlTable table)
    {
        // TODO
        return new GodotConfiguration();
    }
}