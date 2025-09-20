namespace Cyival.Build.Build;

public interface ITargetBuilderBase
{
    Type[] GetRequiredEnvironmentTypes();
    
    Type[] GetRequiredConfigurationTypes();

    void Setup(PathSolver pathSolver, string outPath, IEnumerable<object> environment, IEnumerable<object> configuration);
    
    public BuildResult Build(IBuildTarget target, BuildSettings? buildSettings = null);

    bool CanBuild(IBuildTarget target, BuildSettings? buildSettings = null);
}