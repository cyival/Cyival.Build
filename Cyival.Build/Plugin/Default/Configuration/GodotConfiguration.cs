using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Cyival.Build.Build;
using Cyival.Build.Configuration;

namespace Cyival.Build.Plugin.Default.Configuration;

using Environment;

public struct GodotConfiguration
{
    // TODO: Make all these into class GodotVersionRange.
    public GodotVersion SpecifiedVersion { get; init; }
    public bool IgnorePatch { get; init; }
    public bool RequiredMono { get; init; }

    public Dictionary<BuildSettings.Platform, string> PreferredExportPresets { get; init; } // TODO
    
    public bool IsGodotPack { get; set; }

    public bool CopySharpArtifacts { get; set; }
    
    public string[] CopyArtifactsFilter { get; set; }

    public string? CopyArtifactsTo { get; set; }

    public GodotInstance? SelectMatchOne(IEnumerable<GodotInstance> instances)
    {
        var ver = SpecifiedVersion;
        GodotInstance selectMatchOne;
        if (RequiredMono)
        {
            instances = instances.Where(t => t.Mono);
        }
        if (!IgnorePatch)
        {
            selectMatchOne = instances.FirstOrDefault(t=> t.Version == ver);
        }
        else
        {
            var matchInstances = instances.Where(t => t.Version.Major == ver.Major && 
                                                              t.Version.Minor == ver.Minor).OrderBy(t => t.Version);
            selectMatchOne = matchInstances.FirstOrDefault();
        }

        if (selectMatchOne == default)
            return null;
        return selectMatchOne;
    }

    // For debugging
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}