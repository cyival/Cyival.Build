using Tomlyn.Model;

namespace Cyival.Build.Configuration;

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