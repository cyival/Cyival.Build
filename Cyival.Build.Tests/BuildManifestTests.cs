namespace Cyival.Build.Tests;

using Configuration;
using Configuration.Dependencies;
using Build;

public class BuildManifestTests
{
    internal class SimpleTarget(string id, IEnumerable<string>? requirements=null) : IBuildTarget
    {
        public string Path { get; } = "";
        public string Id { get; } = id;
        public List<string> Requirements { get; } = requirements?.ToList() ?? [];
        
        public void SetLocalConfiguration<T>(T configuration)
        {
            throw new NotImplementedException();
        }

        public T? GetLocalConfiguration<T>()
        {
            throw new NotImplementedException();
        }
    }
    
    [Fact]
    public void AddTarget_ValidTarget_ShouldAddToManifest()
    {
        // Arrange
        var manifest = new BuildManifest();
        var target = new SimpleTarget("test", ["dependency"]);
        
        // Act
        manifest.AddTarget(target);
        
        // Assert
        var retrievedTarget = manifest.GetTarget("test");
        Assert.NotNull(retrievedTarget);
        Assert.Equal("test", retrievedTarget.Id);
    }

    [Fact]
    public void AddTarget_DuplicateId_ShouldThrowArgumentException()
    {
        // Arrange
        var manifest = new BuildManifest();
        var target1 = new SimpleTarget("test", null);
        var target2 = new SimpleTarget("test", null);
        
        // Act
        manifest.AddTarget(target1);
        
        // Assert
        var ex = Assert.Throws<ArgumentException>(() => manifest.AddTarget(target2));
        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public void AddTarget_NullTarget_ShouldThrowArgumentNullException()
    {
        // Arrange
        var manifest = new BuildManifest();
        
        // Act & Assert
        #pragma warning disable CS8625
        Assert.Throws<ArgumentNullException>(() => manifest.AddTarget(null));
        #pragma warning restore CS8625
    }

    [Fact]
    public void CheckDependencies_NoDependencies_ShouldPass()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A"));
        manifest.AddTarget(new SimpleTarget("B"));
        manifest.AddTarget(new SimpleTarget("C"));
        
        // Act & Assert (should not throw)
        manifest.CheckDependencies();
    }

    [Fact]
    public void CheckDependencies_ValidDependencies_ShouldPass()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A"));
        manifest.AddTarget(new SimpleTarget("B", ["A"]));
        manifest.AddTarget(new SimpleTarget("C", ["B"]));
        
        // Act & Assert (should not throw)
        manifest.CheckDependencies();
    }

    [Fact]
    public void CheckDependencies_SelfReference_ShouldThrowDependencyValidationException()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A", ["A"]));
        
        // Act & Assert
        var ex = Assert.Throws<DependencyValidationException>(() => manifest.CheckDependencies());
        Assert.Single(ex.Errors);
        Assert.Equal(DependencyErrorType.SelfReference, ex.Errors[0].ErrorType);
        Assert.Contains("A", ex.Errors[0].ToString());
    }

    [Fact]
    public void CheckDependencies_InvalidReference_ShouldThrowDependencyValidationException()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A", ["nonexistent"]));
        
        // Act & Assert
        var ex = Assert.Throws<DependencyValidationException>(() => manifest.CheckDependencies());
        Assert.Single(ex.Errors);
        Assert.Equal(DependencyErrorType.InvalidReference, ex.Errors[0].ErrorType);
        Assert.Contains("nonexistent", ex.Errors[0].ToString());
    }

    [Fact]
    public void CheckDependencies_CircularReference_ShouldThrowDependencyValidationException()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A", ["B"]));
        manifest.AddTarget(new SimpleTarget("B", ["C"]));
        manifest.AddTarget(new SimpleTarget("C", ["A"]));
        
        // Act & Assert
        var ex = Assert.Throws<DependencyValidationException>(() => manifest.CheckDependencies());
        Assert.Single(ex.Errors);
        Assert.Equal(DependencyErrorType.CircularReference, ex.Errors[0].ErrorType);
        Assert.Contains("Circular reference", ex.Errors[0].ToString());
    }

    [Fact]
    public void CheckDependencies_MultipleErrors_ShouldReportAllErrors()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A", ["A"])); // Self-reference
        manifest.AddTarget(new SimpleTarget("B", ["nonexistent"])); // Invalid reference
        manifest.AddTarget(new SimpleTarget("C", ["D"]));
        manifest.AddTarget(new SimpleTarget("D", ["C"])); // Circular reference
        
        // Act & Assert
        var ex = Assert.Throws<DependencyValidationException>(() => manifest.CheckDependencies());
        Assert.Equal(3, ex.Errors.Count);
        
        var selfRefCount = ex.Errors.Count(e => e.ErrorType == DependencyErrorType.SelfReference);
        var invalidRefCount = ex.Errors.Count(e => e.ErrorType == DependencyErrorType.InvalidReference);
        var circularRefCount = ex.Errors.Count(e => e.ErrorType == DependencyErrorType.CircularReference);
        
        Assert.Equal(1, selfRefCount);
        Assert.Equal(1, invalidRefCount);
        Assert.Equal(1, circularRefCount);
    }

    [Fact]
    public void GetOrderedTargets_NoDependencies_ShouldReturnAllTargetsInAnyOrder()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A"));
        manifest.AddTarget(new SimpleTarget("B"));
        manifest.AddTarget(new SimpleTarget("C"));
        
        // Act
        var ordered = manifest.GetOrderedTargets().ToList();
        
        // Assert
        Assert.Equal(3, ordered.Count);
        Assert.Contains(ordered, t => t.Id == "A");
        Assert.Contains(ordered, t => t.Id == "B");
        Assert.Contains(ordered, t => t.Id == "C");
    }

    [Fact]
    public void GetOrderedTargets_LinearDependencies_ShouldReturnCorrectOrder()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A"));
        manifest.AddTarget(new SimpleTarget("B", ["A"]));
        manifest.AddTarget(new SimpleTarget("C", ["B"]));
        
        // Act
        var ordered = manifest.GetOrderedTargets().ToList();
        
        // Assert
        Assert.Equal(3, ordered.Count);
        Assert.Equal("A", ordered[0].Id);
        Assert.Equal("B", ordered[1].Id);
        Assert.Equal("C", ordered[2].Id);
    }

    [Fact]
    public void GetOrderedTargets_BranchingDependencies_ShouldReturnCorrectOrder()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A"));
        manifest.AddTarget(new SimpleTarget("B", ["A"]));
        manifest.AddTarget(new SimpleTarget("C", ["A"]));
        manifest.AddTarget(new SimpleTarget("D", ["B", "C"]));
        
        // Act
        var ordered = manifest.GetOrderedTargets().ToList();
        
        // Assert
        Assert.Equal(4, ordered.Count);
        
        var aIndex = ordered.FindIndex(t => t.Id == "A");
        var bIndex = ordered.FindIndex(t => t.Id == "B");
        var cIndex = ordered.FindIndex(t => t.Id == "C");
        var dIndex = ordered.FindIndex(t => t.Id == "D");
        
        Assert.True(aIndex < bIndex);
        Assert.True(aIndex < cIndex);
        Assert.True(bIndex < dIndex);
        Assert.True(cIndex < dIndex);
    }

    [Fact]
    public void GetOrderedTargets_ComplexDependencies_ShouldReturnCorrectOrder()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("compile"));
        manifest.AddTarget(new SimpleTarget("test-libs", ["compile"]));
        manifest.AddTarget(new SimpleTarget("test-app", ["compile"]));
        manifest.AddTarget(new SimpleTarget("package", ["test-libs", "test-app"]));
        manifest.AddTarget(new SimpleTarget("deploy", ["package"]));
        
        // Act
        var ordered = manifest.GetOrderedTargets().ToList();
        
        // Assert
        var compileIndex = ordered.FindIndex(t => t.Id == "compile");
        var testLibsIndex = ordered.FindIndex(t => t.Id == "test-libs");
        var testAppIndex = ordered.FindIndex(t => t.Id == "test-app");
        var packageIndex = ordered.FindIndex(t => t.Id == "package");
        var deployIndex = ordered.FindIndex(t => t.Id == "deploy");
        
        Assert.True(compileIndex < testLibsIndex);
        Assert.True(compileIndex < testAppIndex);
        Assert.True(testLibsIndex < packageIndex);
        Assert.True(testAppIndex < packageIndex);
        Assert.True(packageIndex < deployIndex);
    }

    [Fact]
    public void GetOrderedTargets_WithCircularDependency_ShouldThrowDependencyValidationException()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A", ["B"]));
        manifest.AddTarget(new SimpleTarget("B", ["C"]));
        manifest.AddTarget(new SimpleTarget("C", ["A"]));
        
        // Act & Assert
        Assert.Throws<DependencyValidationException>(() => manifest.GetOrderedTargets());
    }

    [Fact]
    public void GenerateDependencyGraph_NoDependencies_ShouldGenerateValidGraph()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A"));
        manifest.AddTarget(new SimpleTarget("B"));
        
        // Act
        var graph = new DependencyValidator(manifest.BuildTargets).GenerateDependencyGraph();
        
        // Assert
        Assert.Contains("digraph BuildDependencies", graph);
        Assert.Contains("\"A\"", graph);
        Assert.Contains("\"B\"", graph);
        Assert.DoesNotContain("->", graph); // No dependencies
    }

    [Fact]
    public void GenerateDependencyGraph_WithDependencies_ShouldGenerateValidGraph()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A"));
        manifest.AddTarget(new SimpleTarget("B", ["A"]));
        
        // Act
        var graph = new DependencyValidator(manifest.BuildTargets).GenerateDependencyGraph();
        
        // Assert
        Assert.Contains("digraph BuildDependencies", graph);
        Assert.Contains("\"A\"", graph);
        Assert.Contains("\"B\"", graph);
        Assert.Contains("\"B\" -> \"A\"", graph);
    }

    [Fact]
    public void PrintDependencyTree_ValidRoot_ShouldNotThrow()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A"));
        manifest.AddTarget(new SimpleTarget("B", ["A"]));
        manifest.AddTarget(new SimpleTarget("C", ["B"]));
        
        // Act & Assert (should not throw)
        new DependencyValidator(manifest.BuildTargets).PrintDependencyTree("C");
    }

    [Fact]
    public void PrintDependencyTree_InvalidRoot_ShouldPrintErrorMessage()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A"));
        
        // Act & Assert (should not throw, just print error)
        new DependencyValidator(manifest.BuildTargets).PrintDependencyTree("nonexistent");
    }

    [Fact]
    public void GetAllTargets_AfterAddingTargets_ShouldReturnAllTargets()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new SimpleTarget("A"));
        manifest.AddTarget(new SimpleTarget("B"));
        manifest.AddTarget(new SimpleTarget("C"));
        
        // Act
        var allTargets = manifest.GetAllTargets().ToList();
        
        // Assert
        Assert.Equal(3, allTargets.Count);
        Assert.Contains(allTargets, t => t.Id == "A");
        Assert.Contains(allTargets, t => t.Id == "B");
        Assert.Contains(allTargets, t => t.Id == "C");
    }

    [Fact]
    public void GetTarget_ExistingTarget_ShouldReturnTarget()
    {
        // Arrange
        var manifest = new BuildManifest();
        var target = new SimpleTarget("test", ["dep"]);
        manifest.AddTarget(target);
        
        // Act
        var result = manifest.GetTarget("test");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("test", result.Id);
    }

    [Fact]
    public void GetTarget_NonExistingTarget_ShouldReturnNull()
    {
        // Arrange
        var manifest = new BuildManifest();
        
        // Act
        var result = manifest.GetTarget("nonexistent");
        
        // Assert
        Assert.Null(result);
    }
}