using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Cyival.Build.Environment;

public partial struct GodotInstance
{
    public int Major { get; private set; }
    public int Minor { get; private set; }
    public int Patch { get; private set; }
    
    public GodotChannel Channel { get; private set; } = GodotChannel.Stable;
    public int StatusVersion { get; private set; } = 0;

    public string Path;
    
    public GodotInstance(string version, string path)
    {
        if (ValidatePath(path) is null)
        {
            throw new ArgumentException($"Failed to validate path: {path}", nameof(path));
        }
        Path = path;
        ParseVersion(version);
    }

    public GodotInstance(string path)
    {
        var version = ValidatePath(path);
        if (version is null)
        {
            throw new ArgumentException($"Failed to validate path: {path}", nameof(path));
        }

        Path = path;
        ParseVersion(version);
    }
    
    private void ParseVersion(string version)
    {
        var split = version.TrimStart('v')
            .Replace('-', '.')
            .Split('.');

        Major = int.Parse(split[0]);
        Minor = int.Parse(split[1]);

        // Versions like 4.2
        if (split.Length == 2)
        {
            return;
        }

        var statusString = "";
        // Check whether the third part is a number
        if (!int.TryParse(split[2], out var patch))
        {
            patch = 0;
            statusString = split[2];
        }
        else if (split.Length > 2)
        {
            statusString = split[3];
        }
        else
        {
            statusString = "";
        }

        Patch = patch;

        var channel = DigitsRegex().Replace(statusString, "")
                      ?? throw new FormatException();

        var statusVersionString = channel.Replace(channel, "");
        if (!string.IsNullOrWhiteSpace(statusVersionString))
        {
            StatusVersion = int.Parse(statusVersionString);
        }

        Channel = channel switch
        {
            "dev" => GodotChannel.Dev,
            "beta" => GodotChannel.Beta,
            "rc" => GodotChannel.ReleaseCandidate,
            _ => GodotChannel.Stable
        };
    }

    private static string? ValidatePath(string path)
    {
        try
        {
            var versionString = "";
            using var process = Process.Start(path, "--version");
            
            while (!process.StandardOutput.EndOfStream)
                versionString += process.StandardOutput.ReadLine();

            process.Close();

            return versionString;
        }
        catch
        {
            return null;
        }
    }

    [GeneratedRegex(@"\d")]
    private static partial Regex DigitsRegex();
}