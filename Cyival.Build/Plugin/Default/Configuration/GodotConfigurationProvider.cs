using Tomlyn.Model;
using Cyival.Build.Configuration;

namespace Cyival.Build.Plugin.Default.Configuration;

using Environment;

public class GodotConfigurationProvider : IConfigurationProvider<GodotConfiguration>
{
    public GodotConfiguration GetDefaultConfiguration()
    {
        throw new NotSupportedException("Default is not supported.");
    }

    public GodotConfiguration ParseFromTable(TomlTable table)
    {
        var verString = (string)table["version"];
        var parsedVersion = GodotVersion.Parse(verString);

        var ignorePatch = !table.TryGetValue("ignore_patch_version", out var ignObj) || (bool)ignObj;

        var requiredMono = table.TryGetValue("required_mono", out var monObj) && (bool)monObj;

        return new GodotConfiguration
        {
            SpecifiedVersion = parsedVersion,
            IgnorePatch = ignorePatch,
            RequiredMono = requiredMono,
        };
    }
}