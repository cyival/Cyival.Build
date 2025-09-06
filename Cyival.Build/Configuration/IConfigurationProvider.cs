using Tomlyn.Model;

namespace Cyival.Build.Configuration;

public interface IConfigurationProvider<out T> : IConfigurationProviderBase
    where T : struct
{
    public T GetDefaultConfiguration();

    public T ParseFromTable(TomlTable table);

    object IConfigurationProviderBase.ParseFromTableAsObject(TomlTable table)
        => ParseFromTable(table);
}