using System.Diagnostics;
using Cyival.Build.Environment;
using Microsoft.Extensions.Logging;

namespace Cyival.Build.Plugin.Default.Environment;

public class GodotEnvProvider : IEnvironmentProvider<GodotInstance>
{
    private ILogger _logger = BuildApp.LoggerFactory.CreateLogger<GodotEnvProvider>();

    public bool CanProvide()
    {
        if (!(OperatingSystem.IsWindows() || OperatingSystem.IsLinux())) return false;

        try
        {
            _logger.LogDebug("Running godotenv");
            var startInfo = new ProcessStartInfo("godotenv", "--version")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<GodotInstance> GetEnvironment()
    {
        var basePath = GetGodotEnvInstallDirectory();
        _logger.LogInformation($"{basePath}");
        if (!Directory.Exists(basePath))
            return [];

        _logger.LogInformation($"{GetExecutableList(basePath)}");

        var instances = new List<GodotInstance>();
        foreach (var exePath in GetExecutableList(basePath))
        {
            try
            {
                var instance = new GodotInstance(exePath);
                instances.Add(instance);
            }
            catch (Exception e)
            {
                _logger.LogWarning("Failed to parse godot instance at {path} caused by:\n{exception}", exePath, e);
            }

        }

        return instances;
    }

    private static string GetGodotEnvInstallDirectory()
    {
        // TODO: Support for config Godot.InstallationsPath
        if (OperatingSystem.IsWindows())
        {
            var appdata = System.Environment.GetEnvironmentVariable("APPDATA")
                          ?? throw new InvalidOperationException("Cannot access the APPDATA environment variable.");
            return Path.Combine(appdata, "godotenv", "godot", "versions");
        }

        if (OperatingSystem.IsLinux())
        {
            var home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            var p = Path.Combine(home, ".config", "godotenv", "godot", "versions");
            return p;
        }

        throw new NotSupportedException("Platform is unsupported.");
    }

    /// <summary>
    /// Get a list of full path to executables.
    /// </summary>
    private static List<string> GetExecutableList(string basePath)
    {
        if (OperatingSystem.IsWindows())
            return Directory.EnumerateFiles(basePath, "Godot*.exe", SearchOption.AllDirectories)
                .Where(str => !str.Contains("console"))
                .Where(str => !str.Contains("GodotTools")).ToList();

        if (OperatingSystem.IsLinux())
            return Directory.EnumerateFiles(basePath, "Godot*.x86_64", SearchOption.AllDirectories)
                .Where(str => !str.Contains("GodotTools")).ToList();

        throw new NotSupportedException("Platform is unsupported.");
    }

    // TODO: Support for other platforms
}
