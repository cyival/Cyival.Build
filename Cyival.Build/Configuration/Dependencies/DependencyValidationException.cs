using System.Text;

namespace Cyival.Build.Configuration.Dependencies;

/// <summary>
/// Custom exception for dependency validation errors
/// </summary>
public class DependencyValidationException : Exception
{
    public List<DependencyError> Errors { get; }

    public DependencyValidationException(List<DependencyError> errors) 
        : base($"Found {errors.Count} dependency errors.")
    {
        Errors = errors;
    }

    // a QOL constructor for single errors
    public DependencyValidationException(DependencyError error)
    {
        Errors = [error];
    }

    /// <summary>
    /// Returns a formatted error report
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(base.Message);
        foreach (var error in Errors)
        {
            sb.AppendLine($"  - {error}");
        }
        return sb.ToString();
    }
}