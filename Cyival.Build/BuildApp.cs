using System.Reflection;
using Cyival.Build.Build;
using Cyival.Build.Configuration;
using Cyival.Build.Environment;
using Cyival.Build.Plugin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cyival.Build;

public sealed class BuildApp(BuildSettings settings) : IDisposable
{
    // TODO: make this configurable
    public const string OutTempDirName = ".cybuild";

    public static ILoggerFactory LoggerFactory { get; set; }
        = NullLoggerFactory.Instance;

    public static TextWriter ConsoleRedirector { get; set; }
        = TextWriter.Null;

    private ILogger<BuildApp> _logger = LoggerFactory.CreateLogger<BuildApp>();

    private PluginStore _pluginStore = new PluginStore();

    private List<object> _environments = [];

    private HashSet<ITargetBuilderBase> _builders = [];

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

    public void CollectItems(string? targetId = null)
    {
        if (Manifest is null)
            throw new InvalidOperationException("BuildApp is not initialized. Call Initialize() first.");

        var targetTypes = Manifest.BuildTargets
            .Select(t => _pluginStore.GetTargetTypeIdByType(t.GetType()) ?? throw new NullReferenceException())
            .ToHashSet(); // This should make same ids into a single one.

        var builders = new HashSet<ITargetBuilderBase>();
        var environments = new List<object>();

        foreach (var typeId in targetTypes)
        {
            // Get builder
            var builder = _pluginStore.GetBuilderByTargetTypeId(typeId);
            builders.Add(builder);

            var environmentTypes = builder.GetRequiredEnvironmentTypes();

            foreach (var envType in environmentTypes)
            {
                if (environments.Any(e => e.GetType() == envType))
                    continue;

                var providers = _pluginStore.GetEnvironmentProvidersByType(envType);

                providers.Where(p => p.CanProvide()).ToList().ForEach(p =>
                {
                    environments = [.. environments, .. p.GetEnvironmentAsObject()];
                });
            }
        }

        _builders = builders;
        _environments = environments;
    }

    public TargetBuildApp Build(string? targetId, string outPath)
    {
        if (Manifest is null)
            throw new InvalidOperationException("BuildApp is not initialized. Call Initialize() first.");

        if (string.IsNullOrEmpty(targetId))
            targetId = Manifest.GetDefaultTargetId()
                       ?? throw new InvalidOperationException("No default targets or more than one default target defined. Please specify a target to build.");

        var buildList = Manifest.GetOrderedTargets(targetId);

        if (_builders.Count == 0) // TODO: check environment count
            throw new InvalidOperationException();

        // check for missing required configuration
        var configurations = GetRequiredConfigurations();

        // TODO: check for whether environment is provided.

        var pathSolver = new PathSolver(Manifest.ManifestPath);

        foreach (var builder in _builders)
        {
            builder.Setup(_environments, configurations);
        }

        return new TargetBuildApp(buildList, _builders, _environments, settings);
    }

    public ManifestParser CreateManifestParser(string? defaultTargetTypeId = null) => new(_pluginStore, defaultTargetTypeId);

    public List<object> GetRequiredConfigurations()
    {
        if (Manifest is null)
            throw new InvalidOperationException("BuildApp is not initialized. Call Initialize() first.");

        var configurations = Manifest.GlobalConfigurations;
        var cfgProviders = _pluginStore.GetConfigurationProviders().Values.ToList();

        foreach (var builder in _builders)
        {
            foreach (var type in builder.GetRequiredConfigurationTypes())
            {
                if (configurations.All(cfg => cfg.GetType() != type))
                {
                    configurations.Add(
                        cfgProviders.First(p => p.ProvidedType == type)
                            .GetDefaultConfigurationAsObject());
                }
            }
        }

        return configurations;
    }
}
