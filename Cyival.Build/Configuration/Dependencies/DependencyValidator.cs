using System.Text;
using Cyival.Build.Build;

namespace Cyival.Build.Configuration.Dependencies;

public class DependencyValidator
{
    private readonly List<IBuildTarget> _targets;
    private readonly Dictionary<string, IBuildTarget> _targetDictionary;

    public DependencyValidator(List<IBuildTarget> targets)
    {
        _targets = targets;
        _targetDictionary = targets.ToDictionary(t => t.Id, t => t);
    }

    public void CheckDependencies()
    {
        var errors = new List<DependencyError>();

        foreach (var target in _targets)
        {
            // Check self-references
            if (target.Requirements.Contains(target.Id))
            {
                errors.Add(new DependencyError(
                    DependencyErrorType.SelfReference,
                    target.Id));
            }

            // Check invalid references
            foreach (var requiredId in target.Requirements)
            {
                if (!_targetDictionary.ContainsKey(requiredId))
                {
                    errors.Add(new DependencyError(
                        DependencyErrorType.InvalidReference,
                        target.Id,
                        requiredId));
                }
            }
        }

        // Check cyclic errors
        var cyclicErrors = FindCyclicDependencies();
        errors.AddRange(cyclicErrors);

        if (errors.Count != 0)
        {
            throw new DependencyValidationException(errors);
        }
    }
    
    /// <summary>
    /// Gets the build order, optionally for a specific target
    /// </summary>
    /// <param name="targetId">ID of the target to build, or null to build all targets</param>
    /// <returns>List of targets in dependency order</returns>
    public IEnumerable<IBuildTarget> GetBuildOrder(string? targetId = null)
    {
        if (string.IsNullOrEmpty(targetId))
        {
            // Build all targets
            return GetFullBuildOrder();
        }
        else
        {
            // Build specific target and its dependencies
            return GetTargetBuildOrder(targetId);
        }
    }

    /// <summary>
    /// Gets the build order for all targets
    /// </summary>
    private IEnumerable<IBuildTarget> GetFullBuildOrder()
    {
        var result = new List<IBuildTarget>();
        var visited = new HashSet<string>();
        var tempMark = new HashSet<string>();

        foreach (var target in _targets)
        {
            if (!visited.Contains(target.Id))
            {
                Visit(target, visited, tempMark, result);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the build order for a specific target and its dependencies
    /// </summary>
    /// <param name="targetId">ID of the target to build</param>
    private IEnumerable<IBuildTarget> GetTargetBuildOrder(string targetId)
    {
        if (!_targetDictionary.TryGetValue(targetId, out var target))
        {
            throw new ArgumentException($"Target with ID '{targetId}' does not exist.");
        }

        // Collect all required targets (specified target and all its dependencies)
        var neededTargets = CollectDependencies(target);
        
        // Perform topological sort on the required targets
        return GetPartialBuildOrder(neededTargets);
    }

    /// <summary>
    /// Collects all dependencies of the specified target
    /// </summary>
    private HashSet<IBuildTarget> CollectDependencies(IBuildTarget target)
    {
        var dependencies = new HashSet<IBuildTarget>();
        var stack = new Stack<IBuildTarget>();
        stack.Push(target);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!dependencies.Add(current))
                continue;

            // Add all dependencies and check if they exist
            foreach (var requiredId in current.Requirements)
            {
                if (!_targetDictionary.TryGetValue(requiredId, out var dependency))
                {
                    throw new DependencyValidationException(
                        new DependencyError(
                            DependencyErrorType.InvalidReference,
                            current.Id,
                            requiredId)
                    );
                }
                stack.Push(dependency);
            }
        }

        return dependencies;
    }

    /// <summary>
    /// Performs topological sort on a subset of targets
    /// </summary>
    private IEnumerable<IBuildTarget> GetPartialBuildOrder(HashSet<IBuildTarget> targets)
    {
        var result = new List<IBuildTarget>();
        var visited = new HashSet<string>();
        var tempMark = new HashSet<string>();

        // Only sort the required targets
        foreach (var target in targets)
        {
            if (!visited.Contains(target.Id))
            {
                Visit(target, visited, tempMark, result, targets);
            }
        }

        return result;
    }

    /// <summary>
    /// Visits a target and processes its dependencies (supports partial target set)
    /// </summary>
    private void Visit(
        IBuildTarget target, 
        ISet<string> visited, 
        ISet<string> tempMark, 
        List<IBuildTarget> result,
        HashSet<IBuildTarget>? allowedTargets = null)
    {
        if (tempMark.Contains(target.Id))
        {
            throw new DependencyValidationException(
                new DependencyError(
                    DependencyErrorType.CircularReference, 
                    target.Id, 
                    additionalInfo: $"Circular dependency detected: {string.Join("->", tempMark)}->{target.Id}")
            );
        }

        if (visited.Contains(target.Id))
        {
            return;
        }

        tempMark.Add(target.Id);

        // 先访问所有依赖项（如果依赖项在允许的目标集合中）
        foreach (var requiredId in target.Requirements)
        {
            if (_targetDictionary.TryGetValue(requiredId, out var dependency) &&
                (allowedTargets == null || allowedTargets.Contains(dependency)))
            {
                Visit(dependency, visited, tempMark, result, allowedTargets);
            }
        }

        tempMark.Remove(target.Id);
        visited.Add(target.Id);
        result.Add(target);
    }
    
    private List<DependencyError> FindCyclicDependencies()
    {
        var errors = new List<DependencyError>();
        var visited = new HashSet<string>();
    
        foreach (var target in _targets)
        {
            // Skip self-reference targets，because they have already checked in CheckDependencies.
            if (target.Requirements.Contains(target.Id))
                continue;

            if (visited.Contains(target.Id))
                continue;
            
            var path = new Stack<string>();
            var recursionStack = new HashSet<string>();
            if (FindCycles(target, visited, recursionStack, path))
            {
                var cyclePath = string.Join(" -> ", path.Reverse());
                errors.Add(new DependencyError(
                    DependencyErrorType.CircularReference, 
                    target.Id, 
                    requiredId: null, 
                    additionalInfo: $"Circular reference: {cyclePath}"));
            }
        }

        return errors;
    }

    private bool FindCycles(IBuildTarget target, ISet<string> visited, ISet<string> recursionStack, Stack<string> path)
    {
        if (target.Requirements.Count == 0)
            return false;

        path.Push(target.Id);

        if (recursionStack.Contains(target.Id))
        {
            // Found a cycle, add the first dependency to complete the cycle display
            path.Push(target.Requirements.First());
            return true;
        }

        // if the target has already been visited, skip it
        if (!visited.Add(target.Id))
        {
            path.Pop();
            return false;
        }

        recursionStack.Add(target.Id);

        foreach (var requiredId in target.Requirements)
        {
            if (_targetDictionary.TryGetValue(requiredId, out var dependency) && 
                FindCycles(dependency, visited, recursionStack, path))
            {
                return true;
            }
        }

        path.Pop();
        recursionStack.Remove(target.Id);
        return false;
    }

    /// <summary>
    /// Generates a DOT format string representing the dependency graph.
    /// (Debugging only)
    /// This can be used with tools like Graphviz to visualize the dependencies.
    /// </summary>
    /// <returns></returns>
    public string GenerateDependencyGraph()
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph BuildDependencies {");
        
        foreach (var target in _targets)
        {
            if (target.Requirements.Count == 0)
            {
                sb.AppendLine($"  \"{target.Id}\";");
            }
            else
            {
                foreach (var requiredId in target.Requirements)
                {
                    sb.AppendLine($"  \"{target.Id}\" -> \"{requiredId}\";");
                }
            }
        }
        
        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Visualizes the dependency tree starting from the specified root target ID.
    /// (Debugging only)
    /// </summary>
    public void PrintDependencyTree(string rootId, int indent = 0)
    {
        if (!_targetDictionary.TryGetValue(rootId, out var target))
        {
            Console.WriteLine($"{new string(' ', indent)}Unknown target: {rootId}");
            return;
        }

        Console.WriteLine($"{new string(' ', indent)}{target.Id}");
        
        foreach (var requiredId in target.Requirements)
        {
            PrintDependencyTree(requiredId, indent + 2);
        }
    }   
}