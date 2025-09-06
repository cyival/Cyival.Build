using Tomlyn.Model;

namespace Cyival.Build.Configuration;

public interface IConfigurationProviderBase
{
    public object ParseFromTableAsObject(TomlTable table);
}