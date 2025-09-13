using Tomlyn.Model;

namespace Cyival.Build.Configuration;

public interface IConfigurationProvider<out T> : IConfigurationProviderBase
    where T : struct
{
    Type IConfigurationProviderBase.ProvidedType => typeof(T);
    
    T GetDefaultConfiguration();

    object IConfigurationProviderBase.GetDefaultConfigurationAsObject() 
        => GetDefaultConfiguration();

    T ParseFromTable(TomlTable table);

    object IConfigurationProviderBase.ParseFromTableAsObject(TomlTable table)
        => ParseFromTable(table);
}