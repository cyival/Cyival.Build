namespace Cyival.Build.Configuration.Dependencies;

/// <summary>
/// Structured dependency error information container
/// </summary>
public class DependencyError(
    DependencyErrorType errorType,
    string targetId,
    string? requiredId = null,
    string? additionalInfo = null)
{
    public DependencyErrorType ErrorType { get; } = errorType;
    public string TargetId { get; } = targetId;
    public string? RequiredId { get; } = requiredId;
    public string? AdditionalInfo { get; } = additionalInfo;

    /// <summary>
    /// Returns a formatted error message
    /// </summary>
    public override string ToString()
    {
        return ErrorType switch
        {
            DependencyErrorType.SelfReference => 
                $"Self-reference error: Target '{TargetId}' references itself",
            DependencyErrorType.InvalidReference => 
                $"Invalid reference error: Target '{TargetId}' references non-existent target '{RequiredId}'",
            DependencyErrorType.CircularReference => 
                $"Circular reference error: {AdditionalInfo}",
            _ => $"Unknown error: Target '{TargetId}', Reference '{RequiredId}', Additional info: {AdditionalInfo}"
        };
    }
}