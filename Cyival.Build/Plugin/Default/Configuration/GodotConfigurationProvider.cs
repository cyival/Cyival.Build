using Tomlyn.Model;
using Cyival.Build.Configuration;
using Microsoft.Extensions.Logging;

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
        GodotVersion parsedVersion = null;
        if (table.TryGetValue("version", out var verObj))
        {
            var verString = (string)verObj;
            parsedVersion = GodotVersion.Parse(verString);
        }

        // Default: true
        var ignorePatch = !table.TryGetValue("ignore_patch_version", out var ignObj) || (bool)ignObj;

        // Default: false
        var requiredMono = table.TryGetValue("required_mono", out var monObj) && (bool)monObj;
        
        // Default: false
        var isGodotPack = table.TryGetValue("export_pack", out var packObj) && (bool)packObj;
        
        BuildApp.LoggerFactory.CreateLogger("GodotConfigurationProvider")
            .LogDebug("Parsed Godot configuration: Version={Version}, IgnorePatch={IgnorePatch}, RequiredMono={RequiredMono}, IsGodotPack={IsGodotPack}",
                parsedVersion?.ToString() ?? "null", ignorePatch, requiredMono, isGodotPack);

        return new GodotConfiguration
        {
            SpecifiedVersion = parsedVersion,
            IgnorePatch = ignorePatch,
            RequiredMono = requiredMono,
            IsGodotPack = isGodotPack,
            PreferredExportPresets = [],
        };
    }
}