using Cyival.Build.Configuration;
using Tomlyn.Model;

namespace Cyival.Build.Plugin.Default.Configuration;

public class CopyOnlyConfigurationProvider : IConfigurationProvider<CopyOnlyConfiguration>
{
    public CopyOnlyConfiguration GetDefaultConfiguration()
    {
        return new CopyOnlyConfiguration();
    }

    public CopyOnlyConfiguration ParseFromTable(TomlTable table)
    {
        throw new NotImplementedException();
    }
}