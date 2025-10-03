using System.Runtime.InteropServices;

namespace Cyival.Build.Build;

public struct BuildSettings(string outPath, string manifestDir)
{
    public required Architecture TargetArchitecture;
    
    public required Platform TargetPlatform;

    public required Mode BuildMode;
    
    public PathSolver OutPathSolver = new PathSolver(outPath);

    public PathSolver ManifestDirSolver = new PathSolver(manifestDir);

    public PathSolver OutTempPathSolver = new PathSolver(outPath).GetSubSolver(BuildApp.OutTempDirName);

    // These should be set before building, after build app initialized.
    // They're variant for different targets.
    public PathSolver SourcePathSolver;
    public PathSolver DestinationPathSolver;
    
    public string? CurrentBuildingTarget;

    public string GlobalSourcePath => SourcePathSolver.GetBasePath();
    public string GlobalDestinationPath => DestinationPathSolver.GetBasePath();
    
    public static Platform GetCurrentPlatform() => System.Environment.OSVersion.Platform switch
    {
        PlatformID.Win32NT => Platform.Windows,
        PlatformID.Unix => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Platform.MacOS : Platform.Linux,
        PlatformID.MacOSX => Platform.MacOS,
        _ => throw new NotSupportedException("Unsupported platform")
    };

    public static Platform ParsePlatformName(string name) => name switch
    {
        "windows" or "win" => Platform.Windows,
        "linux" => Platform.Linux,
        "mac" or "macos" or "osx" => Platform.Linux,
        _ => throw new NotSupportedException("Unsupported platform")
    };

    public bool IsBuilding(IBuildTarget target) =>
        CurrentBuildingTarget is not null && CurrentBuildingTarget == target.Id;
    
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

    public enum Mode
    {
        Release,
        Debug,
    }
}