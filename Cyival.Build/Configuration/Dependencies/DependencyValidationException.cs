using System.Text;

namespace Cyival.Build.Configuration.Dependencies;

public class DependencyValidationException : Exception
{
    public List<DependencyError> Errors { get; }

    public DependencyValidationException(List<DependencyError> errors) 
        : base($"Found {errors.Count} dependency errors.")
    {
        Errors = errors;
    }

    public DependencyValidationException(DependencyError error)
    {
        Errors = [error];
    }

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