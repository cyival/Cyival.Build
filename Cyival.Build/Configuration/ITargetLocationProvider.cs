
namespace Cyival.Build.Configuration;

public interface ITargetLocationProvider
{
    Type ProvidedType { get; }

    string KeyNameOfProvidedType { get; }

    bool CanProvide(object locationObject);

    ITargetLocation Parse(object locationObject, string manifestDir);
}
