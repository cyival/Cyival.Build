namespace Cyival.Build.Environment;

public interface IEnvironmentProviderBase
{
    Type ProvidedType { get; }

    bool CanProvide();
}