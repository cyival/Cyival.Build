using System.Text.RegularExpressions;

namespace Cyival.Build.Plugin.Default.Environment;

public record GodotVersion(int Major, int Minor, int Patch = 0, 
    GodotChannel Channel = GodotChannel.Stable, int StatusVersion = 0) : IComparable<GodotVersion>
{
    public int CompareTo(GodotVersion? other)
    {
        if (other is null)
            return 1;
        
        var majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0) return majorComparison;
        var minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0) return minorComparison;
        var patchComparison = Patch.CompareTo(other.Patch);
        if (patchComparison != 0) return patchComparison;
        var channelComparison = Channel.CompareTo(other.Channel);
        if (channelComparison != 0) return channelComparison;
        return StatusVersion.CompareTo(other.StatusVersion);
    }
    
    public static GodotVersion Parse(string version)
    {
        ArgumentException.ThrowIfNullOrEmpty(version, nameof(version));

        // Remove leading 'v' and split by dots and hyphens
        var parts = version.TrimStart('v').Split(['.', '-']);
    
        if (parts.Length < 2)
            throw new FormatException("Invalid version format. Expected at least major.minor");

        // Parse major and minor versions
        if (!int.TryParse(parts[0], out int major) || !int.TryParse(parts[1], out int minor))
            throw new FormatException("Major and minor versions must be integers");

        int patch = 0;
        GodotChannel channel = GodotChannel.Stable;
        int statusVersion = 0;

        // Handle different version formats
        if (parts.Length >= 3)
        {
            // Check if the third part is a patch number
            if (int.TryParse(parts[2], out patch))
            {
                // Look for channel information in subsequent parts
                foreach (var part in parts[3..])
                {
                    if (TryParseChannelAndStatus(part, out channel, out statusVersion))
                        break; // Found channel info, stop processing
                }
            }
            else if (!TryParseChannelAndStatus(parts[2], out channel, out statusVersion))
            {
                throw new FormatException($"Invalid patch number or channel: {parts[2]}");
            }
        }

        return new GodotVersion(major, minor, patch, channel, statusVersion);
    }

    private static bool TryParseChannelAndStatus(string input, out GodotChannel channel, out int statusVersion)
    {
        statusVersion = 0;
        channel = GodotChannel.Stable;
        
        // Skip empty parts
        if (string.IsNullOrWhiteSpace(input))
            return false;
        
        // Use regex to separate channel name from status version
        var match = Regex.Match(input, @"^([a-zA-Z]+)(\d*)$");
        
        if (!match.Success)
            return input.Equals("stable", StringComparison.OrdinalIgnoreCase);

        string channelName = match.Groups[1].Value.ToLower();
        string statusString = match.Groups[2].Value;

        channel = channelName switch
        {
            "dev" => GodotChannel.Dev,
            "beta" => GodotChannel.Beta,
            "rc" => GodotChannel.ReleaseCandidate,
            "stable" => GodotChannel.Stable,
            _ => (GodotChannel)(-1) // Invalid channel
        };
            
        if (channel == (GodotChannel)(-1))
            return false;

        return string.IsNullOrEmpty(statusString) || int.TryParse(statusString, out statusVersion);
    }
}