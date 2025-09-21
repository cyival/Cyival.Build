using System.Diagnostics;

namespace Cyival.Build.Plugin.Default.Environment;

public partial record struct GodotInstance
{
    public GodotVersion Version { get; private set; }

    public string Path;

    public bool Mono { get; private set; }
    
    public GodotInstance(GodotVersion version, string path)
    {
        if (ValidatePath(path) is null)
        {
            throw new ArgumentException($"Failed to validate path: {path}", nameof(path));
        }
        Path = path;
        Version = version;
    }

    public GodotInstance(string path)
    {
        var versionString = ValidatePath(path);
        if (versionString is null)
        {
            throw new ArgumentException($"Failed to validate path: {path}", nameof(path));
        }

        Path = path;
        Version = GodotVersion.Parse(versionString);
        Mono = versionString.Contains("mono") || versionString.Contains("dotnet");
    }

    private static string? ValidatePath(string path)
    {
        try
        {
            var versionString = "";
            var startInfo = new ProcessStartInfo(path, "--version")
            {
                RedirectStandardOutput = true
            };
            using var process = Process.Start(startInfo);
            
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
}