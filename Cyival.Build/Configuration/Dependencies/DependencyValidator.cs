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

    
    public List<IBuildTarget> GetBuildOrder()
    {
        var visited = new HashSet<string>();
        var tempMark = new HashSet<string>();
        var result = new List<IBuildTarget>();

        foreach (var target in _targets)
        {
            if (visited.Contains(target.Id))
                continue;
            
            Visit(target, visited, tempMark, result);
        }

        var cyclicErrors = FindCyclicDependencies();
        return cyclicErrors.Count != 0 ? throw new InvalidOperationException(string.Join("\n", cyclicErrors))
             : result;
    }

    private void Visit(IBuildTarget target, ISet<string> visited, ISet<string> tempMark, List<IBuildTarget> result)
    {
        if (tempMark.Contains(target.Id))
        {
            throw new DependencyValidationException(new DependencyError(
                DependencyErrorType.CircularReference,
                target.Id,
                string.Empty, 
                $"Circular dependency detected: {string.Join("->", tempMark)}->{target.Id}"));
        }

        if (visited.Contains(target.Id))
        {
            return;
        }

        tempMark.Add(target.Id);

        // Visit all dependencies first
        foreach (var requiredId in target.Requirements)
        {
            if (_targetDictionary.TryGetValue(requiredId, out var dependency))
            {
                Visit(dependency, visited, tempMark, result);
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
                    null, 
                    $"Circular reference: {cyclePath}"));
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