using System.Reflection;
using Cyival.Build.Build;
using Cyival.Build.Configuration;
using Cyival.Build.Environment;
using Microsoft.Extensions.Logging;

namespace Cyival.Build.Plugin;

public class PluginStore
{
    private ILogger<PluginStore> _logger = BuildApp.LoggerFactory.CreateLogger<PluginStore>();
    private HashSet<string> _initializedPlugins = [];
    
    private Dictionary<string, IEnvironmentProviderBase> _environmentProviders = [];

    private Dictionary<string, IConfigurationProviderBase> _configurationProviders = [];

    private Dictionary<string, ITargetBuilderBase> _targetBuilders = [];
    
    private Dictionary<string, Type> _targetTypes = [];
    
    public void ScanAndInitialize(Assembly[] assemblies)
    {
        // Select all types with PluginAttribute and inherits from Plugin class
        var types = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsDefined(typeof(PluginAttribute), false) &&
                           type is { IsAbstract: false, IsInterface: false })
            .Where(t => t.BaseType == typeof(Plugin))
            .ToList();

        types
            .ForEach(t =>
            {
                var attribute = (PluginAttribute)(Attribute.GetCustomAttribute(t, typeof(PluginAttribute)) ??
                                                  throw new NullReferenceException());

                if (_initializedPlugins.Contains(attribute.Id))
                {
                    _logger.LogWarning("Plugin \"{}\" already initialized.", attribute.Id);
                    return;
                }
                
                if (Activator.CreateInstance(t) is not Plugin plugin)
                {
                    _logger.LogWarning("Failed to initialize plugin");
                    return;
                }
                
                plugin.Initialize(this);

                _initializedPlugins.Add(attribute.Id);
            });
    }

    private void RegisterEnvironmentProvider(string id, IEnvironmentProviderBase provider)
    {
        _environmentProviders.Add(id, provider);
    }
    
    public void RegisterEnvironmentProvider<T>(string id) where T : IEnvironmentProviderBase, new()
    {
        RegisterEnvironmentProvider(id, new T());
    }
    
    private void RegisterConfigurationProvider(string id, IConfigurationProviderBase provider)
    {
        _configurationProviders.Add(id, provider);
    }
    
    public void RegisterConfigurationProvider<T>(string id) where T : IConfigurationProviderBase, new()
    {
        RegisterConfigurationProvider(id, new T());
    }
    
    private void RegisterTargetBuilder(string id, ITargetBuilderBase builder)
    {
        _targetBuilders.Add(id, builder);
    }

    public void RegisterTargetBuilder<T>(string id) where T : ITargetBuilderBase, new()
    {
        RegisterTargetBuilder(id, new T());
    }
    
    private void RegisterTargetType(string id, Type type)
    {
        if (type.GetInterfaces().All(i => i != typeof(IBuildTarget)))
        {
            throw new NotSupportedException();
        }
        
        _targetTypes.Add(id, type);
    }
    
    public void RegisterTargetType<T>(string id) where T : TargetBase, IBuildTarget
    {
        RegisterTargetType(id, typeof(T));
    }

    public Type? GetTargetTypeById(string typeId) => _targetTypes.GetValueOrDefault(typeId);
    
    public Dictionary<string, IConfigurationProviderBase> GetConfigurationProviders() => _configurationProviders;
}