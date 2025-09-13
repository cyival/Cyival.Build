namespace Cyival.Build.Environment;

public interface IEnvironmentProvider<out T> : IEnvironmentProviderBase
{
    Type IEnvironmentProviderBase.ProvidedType => typeof(T);

    IEnumerable<T> GetEnvironment();

    IEnumerable<object> IEnvironmentProviderBase.GetEnvironmentAsObject() => GetEnvironment().OfType<object>();
}