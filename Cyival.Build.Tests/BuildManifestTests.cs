namespace Cyival.Build.Tests;

using Configuration;
using Configuration.Dependencies;
using Build;

internal class MockBuildTarget : TargetBase, IBuildTarget
{
    public MockBuildTarget(string id, IEnumerable<string>? requirements=null)
         : base(string.Empty, id, requirements)
    {
        
    }
    
    public void SetLocalConfiguration<T>(T configuration)
    {
        throw new NotImplementedException();
    }

    public T? GetLocalConfiguration<T>()
    {
        throw new NotImplementedException();
    }
}

public class BuildManifestTests
{
    [Fact]
    public void AddTarget_ValidTarget_ShouldAddToManifest()
    {
        // Arrange
        var manifest = new BuildManifest();
        var target = new MockBuildTarget("test", ["dependency"]);
        
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
        var target1 = new MockBuildTarget("test", null);
        var target2 = new MockBuildTarget("test", null);
        
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
        manifest.AddTarget(new MockBuildTarget("A"));
        manifest.AddTarget(new MockBuildTarget("B"));
        manifest.AddTarget(new MockBuildTarget("C"));
        
        // Act & Assert (should not throw)
        manifest.CheckDependencies();
    }

    [Fact]
    public void CheckDependencies_ValidDependencies_ShouldPass()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new MockBuildTarget("A"));
        manifest.AddTarget(new MockBuildTarget("B", ["A"]));
        manifest.AddTarget(new MockBuildTarget("C", ["B"]));
        
        // Act & Assert (should not throw)
        manifest.CheckDependencies();
    }

    [Fact]
    public void CheckDependencies_SelfReference_ShouldThrowDependencyValidationException()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new MockBuildTarget("A", ["A"]));
        
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
        manifest.AddTarget(new MockBuildTarget("A", ["nonexistent"]));
        
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
        manifest.AddTarget(new MockBuildTarget("A", ["B"]));
        manifest.AddTarget(new MockBuildTarget("B", ["C"]));
        manifest.AddTarget(new MockBuildTarget("C", ["A"]));
        
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
        manifest.AddTarget(new MockBuildTarget("A", ["A"])); // Self-reference
        manifest.AddTarget(new MockBuildTarget("B", ["nonexistent"])); // Invalid reference
        manifest.AddTarget(new MockBuildTarget("C", ["D"]));
        manifest.AddTarget(new MockBuildTarget("D", ["C"])); // Circular reference
        
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
        manifest.AddTarget(new MockBuildTarget("A"));
        manifest.AddTarget(new MockBuildTarget("B"));
        manifest.AddTarget(new MockBuildTarget("C"));
        
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
        manifest.AddTarget(new MockBuildTarget("A"));
        manifest.AddTarget(new MockBuildTarget("B", ["A"]));
        manifest.AddTarget(new MockBuildTarget("C", ["B"]));
        
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
        manifest.AddTarget(new MockBuildTarget("A"));
        manifest.AddTarget(new MockBuildTarget("B", ["A"]));
        manifest.AddTarget(new MockBuildTarget("C", ["A"]));
        manifest.AddTarget(new MockBuildTarget("D", ["B", "C"]));
        
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
        manifest.AddTarget(new MockBuildTarget("compile"));
        manifest.AddTarget(new MockBuildTarget("test-libs", ["compile"]));
        manifest.AddTarget(new MockBuildTarget("test-app", ["compile"]));
        manifest.AddTarget(new MockBuildTarget("package", ["test-libs", "test-app"]));
        manifest.AddTarget(new MockBuildTarget("deploy", ["package"]));
        
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
        manifest.AddTarget(new MockBuildTarget("A", ["B"]));
        manifest.AddTarget(new MockBuildTarget("B", ["C"]));
        manifest.AddTarget(new MockBuildTarget("C", ["A"]));
        
        // Act & Assert
        Assert.Throws<DependencyValidationException>(() => manifest.GetOrderedTargets());
    }

    [Fact]
    public void GenerateDependencyGraph_NoDependencies_ShouldGenerateValidGraph()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new MockBuildTarget("A"));
        manifest.AddTarget(new MockBuildTarget("B"));
        
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
        manifest.AddTarget(new MockBuildTarget("A"));
        manifest.AddTarget(new MockBuildTarget("B", ["A"]));
        
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
        manifest.AddTarget(new MockBuildTarget("A"));
        manifest.AddTarget(new MockBuildTarget("B", ["A"]));
        manifest.AddTarget(new MockBuildTarget("C", ["B"]));
        
        // Act & Assert (should not throw)
        new DependencyValidator(manifest.BuildTargets).PrintDependencyTree("C");
    }

    [Fact]
    public void PrintDependencyTree_InvalidRoot_ShouldPrintErrorMessage()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new MockBuildTarget("A"));
        
        // Act & Assert (should not throw, just print error)
        new DependencyValidator(manifest.BuildTargets).PrintDependencyTree("nonexistent");
    }

    [Fact]
    public void GetAllTargets_AfterAddingTargets_ShouldReturnAllTargets()
    {
        // Arrange
        var manifest = new BuildManifest();
        manifest.AddTarget(new MockBuildTarget("A"));
        manifest.AddTarget(new MockBuildTarget("B"));
        manifest.AddTarget(new MockBuildTarget("C"));
        
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
        var target = new MockBuildTarget("test", ["dep"]);
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

public class BuildManifestTargetSpecificTests
{
    [Fact]
    public void GetBuildOrder_NullTargetId_ShouldReturnAllTargets()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("A"),
            new MockBuildTarget("B"),
            new MockBuildTarget("C")
        };
        var validator = new DependencyValidator(targets);
        
        // Act
        var result = validator.GetBuildOrder(null).ToList();
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, t => t.Id == "A");
        Assert.Contains(result, t => t.Id == "B");
        Assert.Contains(result, t => t.Id == "C");
    }

    [Fact]
    public void GetBuildOrder_EmptyTargetId_ShouldReturnAllTargets()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("A"),
            new MockBuildTarget("B"),
            new MockBuildTarget("C")
        };
        var validator = new DependencyValidator(targets);
        
        // Act
        var result = validator.GetBuildOrder("").ToList();
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, t => t.Id == "A");
        Assert.Contains(result, t => t.Id == "B");
        Assert.Contains(result, t => t.Id == "C");
    }

    [Fact]
    public void GetBuildOrder_NonExistentTargetId_ShouldThrowArgumentException()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("A"),
            new MockBuildTarget("B")
        };
        var validator = new DependencyValidator(targets);
        
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => validator.GetBuildOrder("nonexistent"));
        Assert.Contains("does not exist", ex.Message);
    }

    [Fact]
    public void GetBuildOrder_SingleTargetNoDependencies_ShouldReturnOnlyThatTarget()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("A"),
            new MockBuildTarget("B"),
            new MockBuildTarget("C")
        };
        var validator = new DependencyValidator(targets);
        
        // Act
        var result = validator.GetBuildOrder("B").ToList();
        
        // Assert
        Assert.Single(result);
        Assert.Equal("B", result[0].Id);
    }

    [Fact]
    public void GetBuildOrder_TargetWithDirectDependencies_ShouldReturnTargetAndDependencies()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("A"),
            new MockBuildTarget("B", new[] { "A" }),
            new MockBuildTarget("C")
        };
        var validator = new DependencyValidator(targets);
        
        // Act
        var result = validator.GetBuildOrder("B").ToList();
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Id); // Dependency first
        Assert.Equal("B", result[1].Id); // Then the target
    }

    [Fact]
    public void GetBuildOrder_TargetWithTransitiveDependencies_ShouldReturnAllDependencies()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("A"),
            new MockBuildTarget("B", new[] { "A" }),
            new MockBuildTarget("C", new[] { "B" }),
            new MockBuildTarget("D")
        };
        var validator = new DependencyValidator(targets);
        
        // Act
        var result = validator.GetBuildOrder("C").ToList();
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("A", result[0].Id);
        Assert.Equal("B", result[1].Id);
        Assert.Equal("C", result[2].Id);
        Assert.DoesNotContain(result, t => t.Id == "D"); // Should not include unrelated targets
    }

    [Fact]
    public void GetBuildOrder_TargetWithMultipleDependencies_ShouldReturnAllDependenciesInCorrectOrder()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("A"),
            new MockBuildTarget("B"),
            new MockBuildTarget("C", new[] { "A", "B" }),
            new MockBuildTarget("D")
        };
        var validator = new DependencyValidator(targets);
        
        // Act
        var result = validator.GetBuildOrder("C").ToList();
        
        // Assert
        Assert.Equal(3, result.Count);
        
        var aIndex = result.FindIndex(t => t.Id == "A");
        var bIndex = result.FindIndex(t => t.Id == "B");
        var cIndex = result.FindIndex(t => t.Id == "C");
        
        Assert.True(aIndex < cIndex);
        Assert.True(bIndex < cIndex);
        Assert.DoesNotContain(result, t => t.Id == "D"); // Should not include unrelated targets
    }

    [Fact]
    public void GetBuildOrder_ComplexDependencyGraph_ShouldReturnCorrectOrder()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("compile"),
            new MockBuildTarget("test-libs", new[] { "compile" }),
            new MockBuildTarget("test-app", new[] { "compile" }),
            new MockBuildTarget("package", new[] { "test-libs", "test-app" }),
            new MockBuildTarget("deploy", new[] { "package" }),
            new MockBuildTarget("docs") // Unrelated target
        };
        var validator = new DependencyValidator(targets);
        
        // Act
        var result = validator.GetBuildOrder("deploy").ToList();
        
        // Assert
        // Should not include "docs" as it's not a dependency
        Assert.Equal(5, result.Count);
        Assert.DoesNotContain(result, t => t.Id == "docs");
        
        // Check order
        var compileIndex = result.FindIndex(t => t.Id == "compile");
        var testLibsIndex = result.FindIndex(t => t.Id == "test-libs");
        var testAppIndex = result.FindIndex(t => t.Id == "test-app");
        var packageIndex = result.FindIndex(t => t.Id == "package");
        var deployIndex = result.FindIndex(t => t.Id == "deploy");
        
        Assert.True(compileIndex < testLibsIndex);
        Assert.True(compileIndex < testAppIndex);
        Assert.True(testLibsIndex < packageIndex);
        Assert.True(testAppIndex < packageIndex);
        Assert.True(packageIndex < deployIndex);
    }

    [Fact]
    public void GetBuildOrder_CircularDependency_ShouldThrowDependencyValidationException()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("A", new[] { "B" }),
            new MockBuildTarget("B", new[] { "C" }),
            new MockBuildTarget("C", new[] { "A" })
        };
        var validator = new DependencyValidator(targets);
        
        // Act & Assert
        Assert.Throws<DependencyValidationException>(() => validator.GetBuildOrder("A"));
    }

    [Fact]
    public void GetBuildOrder_SelfReference_ShouldThrowDependencyValidationException()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("A", new[] { "A" })
        };
        var validator = new DependencyValidator(targets);
        
        // Act & Assert
        Assert.Throws<DependencyValidationException>(() => validator.GetBuildOrder("A"));
    }

    [Fact]
    public void GetBuildOrder_InvalidReference_ShouldThrowDependencyValidationException()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("A", new[] { "nonexistent" })
        };
        var validator = new DependencyValidator(targets);
        
        // Act & Assert
        Assert.Throws<DependencyValidationException>(() => validator.GetBuildOrder("A"));
    }
}

public class BuildManifestExtensionTests
{
    [Fact]
    public void GetOrderedTargets_WithTargetId_ShouldReturnSpecificTargetAndDependencies()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("A"),
            new MockBuildTarget("B", new[] { "A" }),
            new MockBuildTarget("C")
        };
        var manifest = new BuildManifest();
        foreach (var target in targets)
        {
            manifest.AddTarget(target);
        }
        
        // Act
        var result = manifest.GetOrderedTargets("B").ToList();
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Id);
        Assert.Equal("B", result[1].Id);
        Assert.DoesNotContain(result, t => t.Id == "C");
    }

    [Fact]
    public void GetOrderedTargets_NoTargetId_ShouldReturnAllTargets()
    {
        // Arrange
        var targets = new List<IBuildTarget>
        {
            new MockBuildTarget("A"),
            new MockBuildTarget("B"),
            new MockBuildTarget("C")
        };
        var manifest = new BuildManifest();
        foreach (var target in targets)
        {
            manifest.AddTarget(target);
        }
        
        // Act
        var result = manifest.GetOrderedTargets().ToList();
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, t => t.Id == "A");
        Assert.Contains(result, t => t.Id == "B");
        Assert.Contains(result, t => t.Id == "C");
    }
}