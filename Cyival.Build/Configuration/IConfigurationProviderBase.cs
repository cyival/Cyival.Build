using Tomlyn.Model;

namespace Cyival.Build.Configuration;

public interface IConfigurationProviderBase
{
    Type ProvidedType { get; }
    
    object ParseAsObject(Dictionary<string, object> data);

    object GetDefaultConfigurationAsObject();
}