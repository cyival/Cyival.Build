using System.Diagnostics;
using Cyival.Build.Environment;
using Microsoft.Extensions.Logging;

namespace Cyival.Build.Plugin.Default.Environment;

public class GodotEnvProvider : IEnvironmentProvider<GodotInstance>
{
    private ILogger _logger = BuildApp.LoggerFactory.CreateLogger<GodotEnvProvider>();
    
    public bool CanProvide()
    {
        if (!OperatingSystem.IsWindows()) return false;
        
        try
        {
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
        // Only for windows now.
        
        var appdata = System.Environment.GetEnvironmentVariable("APPDATA")
                      ?? throw new InvalidOperationException("Cannot access the APPDATA environment variable.");
        var basePath = Path.Combine(appdata, "godotenv", "godot", "versions"); // TODO: Support for config Godot.InstallationsPath
        if (!Directory.Exists(basePath))
            return [];

        // should get a list of full path to executables.
        var exeList = Directory.GetFiles(basePath, "Godot*.exe", SearchOption.AllDirectories)
            .Where(str => !str.Contains("console"))
            .Where(str => !str.Contains("GodotTools")).ToList();

        var instances = new List<GodotInstance>();
        foreach (var exePath in exeList)
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
    
    // TODO: Support for other platforms
}