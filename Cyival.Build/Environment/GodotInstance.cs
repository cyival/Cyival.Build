using System.Diagnostics;

namespace Cyival.Build.Environment;

public partial record struct GodotInstance
{
    public GodotVersion Version { get; private set; }

    public string Path;
    
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
}