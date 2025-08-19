using Cyival.Build.Configuration;
using Cyival.Build.Environment;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cyival.Build;

public class BuildApp : IDisposable
{
    public static ILoggerFactory LoggerFactory { get; set; }
        = NullLoggerFactory.Instance;
    
    private ILogger<BuildApp> _logger = LoggerFactory.CreateLogger<BuildApp>();
    
    private List<IEnvironmentProviderBase> _environmentProviders = [];

    private List<IConfigurationProviderBase> _configurationProviders = [];
    
    public void LoadPlugins()
    {
        // Get all plugins
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsDefined(typeof(BuildPluginAttribute), false))
            .ToList();

        // Register environment providers
        types    
            .Where(type => type is { IsAbstract: false, IsInterface: false } && 
                           type.GetInterfaces()
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
        
        // Register configuration providers
        types    
            .Where(type => type is { IsAbstract: false, IsInterface: false } && 
                           type.GetInterfaces()
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
    }

    public void Dispose()
    {
        // TODO release managed resources here
        GC.SuppressFinalize(this);
    }
}