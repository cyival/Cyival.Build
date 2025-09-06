using Cyival.Build.Build;
using Cyival.Build.Configuration.Dependencies;

namespace Cyival.Build.Configuration;

/// <summary>
/// A manifest that contains a list of build targets and their dependencies.
/// It provides methods to check dependencies and retrieve ordered targets for building.
/// This manifest is used to manage the build process in a structured way, ensuring that all dependencies
/// are resolved before building the targets.
///
/// Use <see cref="ManifestParser"/> to parse a manifest file into this structure.
/// The manifest can be extended with additional properties or methods as needed for specific build requirements.
/// The <see cref="IBuildTarget"/> interface should be implemented by all build targets to
/// ensure they provide the necessary information such as path, ID, and requirements.
/// The <see cref="DependencyValidator"/> class is used to validate the dependencies of the targets
/// and to ensure that there are no cyclic dependencies or invalid references.
/// </summary>
public class BuildManifest
{
    public List<IBuildTarget> BuildTargets { get; } = [];

    public void CheckDependencies()
    {
        var validator = new DependencyValidator(BuildTargets);
        validator.CheckDependencies();
    }

    public void AddTarget(IBuildTarget target)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target), "Target cannot be null.");
        }
        
        if (BuildTargets.Any(t => t.Id == target.Id))
        {
            throw new ArgumentException($"A target with ID '{target.Id}' already exists in the manifest.");
        }
        
        BuildTargets.Add(target);
    }
    
    /// <summary>
    /// Gets targets in dependency order, optionally for a specific target
    /// </summary>
    /// <param name="targetId">ID of the target to build, or null to build all targets</param>
    /// <returns>List of targets in dependency order</returns>
    public IEnumerable<IBuildTarget> GetOrderedTargets(string? targetId = null)
    {
        var validator = new DependencyValidator(BuildTargets);
        return validator.GetBuildOrder(targetId);
    }

    public IBuildTarget? GetTarget(string id) => BuildTargets.FirstOrDefault(t => t.Id == id);

    public IEnumerable<IBuildTarget> GetAllTargets() => BuildTargets;
}