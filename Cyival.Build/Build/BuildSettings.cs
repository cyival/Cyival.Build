using System.Runtime.InteropServices;

namespace Cyival.Build.Build;

public struct BuildSettings
{
    public Architecture TargetArchitecture;
    
    public Platform TargetPlatform;
    
    public static BuildSettings GetCurrentBuildSettings()
    {
        var platform = System.Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => Platform.Windows,
            PlatformID.Unix => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Platform.MacOS : Platform.Linux,
            PlatformID.MacOSX => Platform.MacOS,
            _ => throw new NotSupportedException("Unsupported platform")
        };

        return new BuildSettings()
        {
            TargetArchitecture = RuntimeInformation.OSArchitecture,
            TargetPlatform = platform,
        };
    }
    
    public enum Platform
    {
        Windows,
        Linux,
        MacOS,
        /*Android,
        iOS,
        Web,*/
        // TODO: Support for other platforms (especially mobile and web)
    }
}