using System.Reflection;
using Cyival.Build.Build;
using Cyival.Build.Configuration;
using Cyival.Build.Environment;
using Cyival.Build.Plugin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cyival.Build;

public sealed class BuildApp : IDisposable
{
    public static ILoggerFactory LoggerFactory { get; set; }
        = NullLoggerFactory.Instance;
    
    private ILogger<BuildApp> _logger = LoggerFactory.CreateLogger<BuildApp>();

    private PluginStore _pluginStore = new PluginStore();
    
    /*public void LoadPlugins()
    {
        // Get all types.
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsDefined(typeof(BuildPluginAttribute), false) &&
                           type is { IsAbstract: false, IsInterface: false })
            .ToList();

        // Register environment providers.
        types    
            .Where(type => type.GetInterfaces()
                               .Any(i => i.IsGenericType && 
                                         i.GetGenericTypeDefinition() == typeof(IEnvironmentProvider<>)))
            .ToList()
            .ForEach(type =>
            {
                if (Activator.CreateInstance(type) is not IEnvironmentProviderBase instance)
                {
                    _logger.LogWarning("Failed to create instance of environment provider: {ProviderType}", type.FullName);
                    return;
                }

                _environmentProviders.Add(instance);
                _logger.LogInformation("Loaded environment provider: {ProviderType}", type.FullName);
                
            });
        
        // Register configuration providers.
        types    
            .Where(type => type.GetInterfaces()
                               .Any(i => i.IsGenericType && 
                                         i.GetGenericTypeDefinition() == typeof(IConfigurationProvider<>)))
            .ToList()
            .ForEach(type =>
            {
                if (Activator.CreateInstance(type) is not IConfigurationProviderBase instance)
                {
                    _logger.LogWarning("Failed to create instance of configuration provider: {ProviderType}", type.FullName);
                    return;
                }

                _configurationProviders.Add(instance);
                _logger.LogInformation("Loaded configuration provider: {ProviderType}", type.FullName);
                
            });
        
        // Register target builders.
        types    
            .Where(type => type.GetInterfaces()
                               .Any(i => i.IsGenericType && 
                                         i.GetGenericTypeDefinition() == typeof(ITargetBuilder<>)))
            .ToList()
            .ForEach(type =>
            {
                if (Activator.CreateInstance(type) is not ITargetBuilderBase instance)
                {
                    _logger.LogWarning("Failed to create instance of target builder: {BuilderType}", type.FullName);
                    return;
                }

                _targetBuilders.Add(instance);
                _logger.LogInformation("Loaded target builder: {BuilderType}", type.FullName);
                
            });
        
        types
            .Where(type => type.GetInterfaces()
                                .Any(i => i == typeof(IBuildTarget)))
            .ToList()
            .ForEach(type =>
            {
                var attr = type.GetCustomAttribute<BuildPluginAttribute>();
                if (attr is null || string.IsNullOrWhiteSpace(attr.Id))
                {
                    _logger.LogWarning("Build target {TargetType} is missing BuildPluginAttribute or Id", type.FullName);
                    return;
                }
                
                _targetTypes.Add(attr.Id, type);
            });
    }*/

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public void InitializePlugins() => _pluginStore.ScanAndInitialize(AppDomain.CurrentDomain.GetAssemblies());
    
    public void Initialize(BuildManifest manifest)
    {
        
    }
}