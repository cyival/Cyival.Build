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

    public BuildManifest? Manifest { get; private set; } = null;
    
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public void InitializePlugins() => _pluginStore.ScanAndInitialize(AppDomain.CurrentDomain.GetAssemblies());
    
    public void Initialize(BuildManifest manifest)
    {
        if (!manifest.DependencyCheckPerformed)
            manifest.CheckDependencies();

        Manifest = manifest;
    }
    
    public void Build(string? targetId)
    {
        if (Manifest is null)
            throw new InvalidOperationException("BuildApp is not initialized. Call Initialize() first.");
        
        if (string.IsNullOrEmpty(targetId))
            targetId = Manifest.GetDefaultTargetId()
                       ?? throw new InvalidOperationException("No default targets or more than one default target defined. Please specify a target to build.");

        var buildList = Manifest.GetOrderedTargets(targetId);
        
        Console.WriteLine(string.Join(',', buildList));
    }
    
    public ManifestParser CreateManifestParser(string? defaultTargetTypeId=null) => new ManifestParser(_pluginStore, defaultTargetTypeId);
}