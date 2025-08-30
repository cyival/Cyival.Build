namespace Cyival.Build.Configuration.Dependencies;

public enum DependencyErrorType
{
    SelfReference,
    InvalidReference,
    CircularReference,
    UnknownError,
}

