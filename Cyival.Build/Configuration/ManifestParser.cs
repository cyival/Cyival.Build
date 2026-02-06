using Cyival.Build.Build;
using Cyival.Build.Plugin;
using Microsoft.Extensions.Logging;
using Tomlyn;
using Tomlyn.Model;

namespace Cyival.Build.Configuration;

public class ManifestParser(PluginStore store, string? defaultTargetType = null)
{
    private PluginStore _pluginStore = store;

    private ILogger<ManifestParser> _logger = BuildApp.LoggerFactory.CreateLogger<ManifestParser>();

    private string _manifestDir = "";

    public BuildManifest Parse(string manifestPath)
    {
        // Forced to be absolute path
        manifestPath = Path.GetFullPath(manifestPath);

        _manifestDir = Path.GetDirectoryName(manifestPath) ?? throw new IOException();

        _logger.LogInformation("Reading manifest at {}", manifestPath);

        if (!File.Exists(manifestPath))
        {
            throw new ArgumentException("File not exists.", nameof(manifestPath));
        }

        var manifestString = File.ReadAllText(manifestPath);
        var model = Toml.ToModel(manifestString);

        // Check version
        if (!model.TryGetValue("minimal-version", out var minimalVersionObject))
            throw new InvalidOperationException("No minimal version specified in manifest.");

        var curVer = GetType().Assembly.GetName().Version ?? throw new Exception("Failed to get version of assembly");
        var curVerNum = curVer.Major + curVer.Minor * 0.1;
        var minVer = (double)minimalVersionObject;

        if (minVer > curVerNum)
            throw new NotSupportedException($"At least required version is {minVer}, but installed is {curVerNum}");

        // Parse targets
        if (!model.TryGetValue("targets", out var targetsObj))
            throw new NotSupportedException("Zero targets defined in manifest.");

        var targetsSections = (TomlTable)targetsObj;

        var targets = ParseTargets(targetsSections);

        _logger.LogInformation("Parsed targets: [{}]", string.Join(';', targets.Select(t => $"{t.Id}: {t.GetType().Name}")));

        if (targets.Count == 0)
            throw new NotSupportedException("Zero targets defined in manifest.");

        var globalConfigurations = new List<object>();
        // Parse global configurations if any
        if (model.TryGetValue("build", out var objBuild) && objBuild is TomlTable buildTable)
        {
            globalConfigurations = ParseConfiguration(ParseTableAsDictionary(buildTable));
        }

        var manifest = new BuildManifest
        {
            BuildTargets = targets,
            GlobalConfigurations = globalConfigurations,
            ManifestPath = manifestPath,
        };

        return manifest;
    }

    private List<IBuildTarget> ParseTargets(TomlTable table)
    {
        var list = new List<IBuildTarget>();

        foreach (var (id, value) in table)
        {
            var targetData = ParseTableAsDictionary((TomlTable)value);

            // TODO: Use a more flexible way.

            // Read generic properties
            var dest = targetData.TryGetValue("output", out var destPathObj) ? destPathObj.ToString() :
                    targetData.TryGetValue("out", out var destPathObj2) ? destPathObj2.ToString() : string.Empty;
            var requirements = targetData.TryGetValue("requirements", out var reqObj)
                ? ((IEnumerable<object?>)reqObj).Select(r => r?.ToString() ?? string.Empty).Where(r => !string.IsNullOrEmpty(r)).ToList()
                : [];
            var typeId = (targetData.TryGetValue("type", out var typeObj) ? typeObj?.ToString() : defaultTargetType)
                       ?? throw new NotSupportedException("Cannot determine target type.");
            var isDefault = targetData.TryGetValue("default", out var defaultObj) && defaultObj is true; // defaults to be false

            // Check for required properties
            if (string.IsNullOrWhiteSpace(id))
                throw new NotSupportedException($"One target in manifest does not have a valid ID.");

            // Get target type from plugin store
            var targetType = _pluginStore.GetTargetTypeById(typeId)
                             ?? throw new NotSupportedException($"Target type '{typeId}' is not registered.");

            // Parse target locations.
            var tlp = _pluginStore.GetTargetLocationProviders().Values.Where(
                p => targetData.Keys.Contains(p.KeyNameOfProvidedType) &&
                p.CanProvide(targetData[p.KeyNameOfProvidedType])).Single();
            var tlObj = targetData[tlp.KeyNameOfProvidedType];

            var tl = tlp.Parse(tlObj, _manifestDir);
            if (tl is null)
                throw new ArgumentNullException();

            // Create target instance by using Activator
            // This should match to the class constructor of TargetBase
            var instanceObj = Activator.CreateInstance(targetType, tl, dest, id, requirements);

            // If failed, try using a empty constructor and set properties later
            if (instanceObj is null)
            {
                instanceObj = Activator.CreateInstance(targetType);

                // Get properties and set them
                var propId = targetType.GetProperty("Id");
                var propTl = targetType.GetProperty("TargetLocation");
                var propDest = targetType.GetProperty("DestinationPath");
                var propRequirements = targetType.GetProperty("Requirements");

                if (propId is null || propTl is null || propDest is null || propRequirements is null)
                    throw new NotSupportedException($"Target type '{typeId}' does not have required properties.");

                propId.SetValue(instanceObj, id);
                propTl.SetValue(instanceObj, tl);
                propDest.SetValue(instanceObj, dest);
                propRequirements.SetValue(instanceObj, propRequirements);
            }

            // Finally, check if instance is IBuildTarget
            if (instanceObj is not IBuildTarget target)
                throw new NotSupportedException($"Failed to create instance of target type '{typeId}'.");

            target.IsDefault = isDefault;

            // Don't forget to let the target parse itself from the table
            target.Parse(targetData);

            // And parse configurations if any
            var configurations = ParseConfiguration(targetData);
            configurations.ForEach(cfg => target.SetLocalConfiguration(cfg));

            _logger.LogDebug($"{System.Text.Json.JsonSerializer.Serialize(tl)}");

            list.Add(target);
        }

        return list;
    }

    private List<object> ParseConfiguration(Dictionary<string, object> table)
    {
        var configProviders = _pluginStore.GetConfigurationProviders();
        var list = new List<object>();

        foreach (var id in configProviders.Keys.Where(table.ContainsKey))
        {
            var provider = configProviders[id];
            //var data = ParseTableAsDictionary((TomlTable)table[id]);
            var data = (Dictionary<string, object>)table[id];

            list.Add(provider.ParseAsObject(data));
        }

        return list;
    }

    private Dictionary<string, object> ParseTableAsDictionary(TomlTable table)
    {
        var dict = new Dictionary<string, object>();

        foreach (var (key, value) in table)
        {
            if (value is TomlTable nestedTable)
            {
                dict[key] = ParseTableAsDictionary(nestedTable);
                continue;
            }

            if (value is TomlArray array)
            {
                dict[key] = array.ToArray();
                continue;
            }
            dict[key] = value;
        }

        return dict;
    }
}
