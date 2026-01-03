using Tomlyn.Model;

namespace Cyival.Build.Configuration;

public interface IConfigurationProvider<out T> : IConfigurationProviderBase
    where T : struct
{
    Type IConfigurationProviderBase.ProvidedType => typeof(T);
    
    T GetDefaultConfiguration();

    object IConfigurationProviderBase.GetDefaultConfigurationAsObject() 
        => GetDefaultConfiguration();

    T Parse(Dictionary<string, object> data);

    object IConfigurationProviderBase.ParseAsObject(Dictionary<string, object> data)
        => Parse(data);
}