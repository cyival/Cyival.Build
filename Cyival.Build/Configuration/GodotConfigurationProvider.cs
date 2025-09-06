using Tomlyn.Model;

namespace Cyival.Build.Configuration;

public class GodotConfigurationProvider : IConfigurationProvider<GodotConfiguration>
{
    public GodotConfiguration GetDefaultConfiguration()
    {
        throw new NotImplementedException();
    }

    public GodotConfiguration ParseFromTable(TomlTable table)
    {
        // TODO
        return new GodotConfiguration();
    }
}