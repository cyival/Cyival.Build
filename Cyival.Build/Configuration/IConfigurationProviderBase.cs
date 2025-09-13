using Tomlyn.Model;

namespace Cyival.Build.Configuration;

public interface IConfigurationProviderBase
{
    Type ProvidedType { get; }
    
    object ParseFromTableAsObject(TomlTable table);

    object GetDefaultConfigurationAsObject();
}