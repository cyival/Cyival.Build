namespace Cyival.Build.Build;

public interface ITargetBuilderBase
{
    Type[] GetRequiredEnvironmentTypes();
    
    Type[] GetRequiredConfigurationTypes();

    void Setup(IEnumerable<object> environment, IEnumerable<object> configuration);
    
    public BuildResult Build(IBuildTarget target);
}