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

        var copyArtifacts = false;
        var copyDllFilter = new List<string>();
        string? copyDllTo = null;
        
        if (table.TryGetValue("csharp", out var csTableObj))
        {
            var csharpTable = (TomlTable)csTableObj;
            
            // Default: false
            if (csharpTable.TryGetValue("artifacts", out var copyDllObj))
                copyArtifacts = (bool)copyDllObj;

            if (csharpTable.TryGetValue("artifacts_filter", out var copyDllFilterObj))
            {
                if (copyDllFilterObj is string cdfs)
                    copyDllFilter.Add(cdfs);

                if (copyDllFilterObj is IEnumerable<string> cdfl)
                    copyDllFilter.AddRange(cdfl);
            }

            if (csharpTable.TryGetValue("artifacts_output", out var copyDllDestObj))
                copyDllTo = (string)copyDllDestObj;
        }
        
        return new GodotConfiguration
        {
            SpecifiedVersion = parsedVersion,
            IgnorePatch = ignorePatch,
            RequiredMono = requiredMono,
            IsGodotPack = isGodotPack,
            PreferredExportPresets = [],
            CopySharpArtifacts = copyArtifacts,
            CopyArtifactsFilter = copyDllFilter.ToArray(),
            CopyArtifactsTo = copyDllTo,
        };
    }
}