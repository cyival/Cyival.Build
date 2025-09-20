using Cyival.Build.Build;
using Microsoft.Extensions.Logging;

namespace Cyival.Build.Plugin.Default.Build;

using Configuration;

public class CopyOnlyTargetBuilder : ITargetBuilder<CopyOnlyTarget>
{
    private CopyOnlyConfiguration _configuration;
    private PathSolver _pathSolver;
    private ILogger _logger = BuildApp.LoggerFactory.CreateLogger<CopyOnlyTargetBuilder>();

    private string _outPath;
    
    public Type[] GetRequiredEnvironmentTypes() => [];

    public Type[] GetRequiredConfigurationTypes() => [typeof(CopyOnlyConfiguration)];

    public void Setup(PathSolver pathSolver, string outPath, IEnumerable<object> environment,
        IEnumerable<object> configuration)
    {
        _configuration = configuration.OfType<CopyOnlyConfiguration>().First();
        _pathSolver = pathSolver;
        _outPath = outPath;
    }

    public BuildResult Build(IBuildTarget target, BuildSettings? buildSettings = null)
    {
        var from = _pathSolver.GetPathTo(target.SourcePath);
        var dest = _pathSolver.GetPathTo(_outPath, target.DestinationPath);
        
        _logger.LogDebug("Src -> {f}, Dest -> {d}", from, dest);

        try
        {
            CopyFilesRecursively(from, dest, _configuration.CopyFilters);
            return BuildResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed while building target {id}:\n{exception}", target.Id, e);
            return BuildResult.Failed;
        }
    }
    
    private void CopyFilesRecursively(string sourcePath, string targetPath, string filter)
    {
        // Validate input parameters
        if (string.IsNullOrEmpty(sourcePath))
            throw new ArgumentException("Source path cannot be null or empty.", nameof(sourcePath));
        
        if (string.IsNullOrEmpty(targetPath))
            throw new ArgumentException("Target path cannot be null or empty.", nameof(targetPath));
        
        if (string.IsNullOrEmpty(filter))
            throw new ArgumentException("Filter cannot be null or empty.", nameof(filter));

        if (!Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

        // Create target directory if it doesn't exist
        Directory.CreateDirectory(targetPath);

        // Copy matching files in current directory
        foreach (var filePath in Directory.EnumerateFiles(sourcePath, filter))
        {
            var fileName = Path.GetFileName(filePath);
            var destFile = Path.Combine(targetPath, fileName);
            _logger.LogDebug("Copying file {src} to {dest}", filePath, destFile);
            File.Copy(filePath, destFile, true); // Set last parameter to false to prevent overwriting
        }

        // Recursively copy subdirectories
        foreach (string directoryPath in Directory.EnumerateDirectories(sourcePath))
        {
            var directoryName = Path.GetFileName(directoryPath);
            var destSubDir = Path.Combine(targetPath, directoryName);
            CopyFilesRecursively(directoryPath, destSubDir, filter);
        }
    }
}