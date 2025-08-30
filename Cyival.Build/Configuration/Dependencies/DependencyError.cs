namespace Cyival.Build.Configuration.Dependencies;

public class DependencyError
{
    public DependencyErrorType ErrorType { get; }
    public string TargetId { get; }
    public string RequiredId { get; }
    public string AdditionalInfo { get; }

    public DependencyError(DependencyErrorType errorType, string targetId, string requiredId = null, string additionalInfo = null)
    {
        ErrorType = errorType;
        TargetId = targetId;
        RequiredId = requiredId;
        AdditionalInfo = additionalInfo;
    }

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