using System.Threading;
using Cyival.Build.Cli.Command;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Testing;

namespace Cyival.Build.Tests;

public class CleanCommandTests
{
    [Fact]
    public void Execute_NoManifest_ShouldAbortAndNotDelete()
    {
        // Arrange
        var temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temp);

        var proj = Path.Combine(temp, "project");
        Directory.CreateDirectory(proj);
        Directory.CreateDirectory(Path.Combine(proj, ".cybuild"));

        var app = new CommandAppTester();
        app.SetDefaultCommand<CleanCommand>();

        // Act
        var result = app.Run(temp, "-y");

        // Assert
        Assert.Equal(1, result.ExitCode);
        Assert.True(Directory.Exists(Path.Combine(proj, ".cybuild")));

        // Cleanup
        Directory.Delete(temp, true);
    }

    [Fact]
    public void Execute_WithManifest_DeletesUpToDepthTwoOnly()
    {
        // Arrange
        var temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temp);

        // place manifest in root
        File.WriteAllText(Path.Combine(temp, "build.toml"), "minimal-version = 0.1\n");

        // top-level project
        var proj1 = Path.Combine(temp, "proj1");
        Directory.CreateDirectory(proj1);
        Directory.CreateDirectory(Path.Combine(proj1, ".cybuild"));

        // depth 2 project
        var sub = Path.Combine(temp, "subdir");
        Directory.CreateDirectory(sub);
        var proj2 = Path.Combine(sub, "proj2");
        Directory.CreateDirectory(proj2);
        Directory.CreateDirectory(Path.Combine(proj2, ".cybuild"));

        // depth 3 project (should NOT be deleted)
        var deep = Path.Combine(temp, "deep");
        Directory.CreateDirectory(deep);
        var deep2 = Path.Combine(deep, "deeper");
        Directory.CreateDirectory(deep2);
        var proj3 = Path.Combine(deep2, "proj3");
        Directory.CreateDirectory(proj3);
        Directory.CreateDirectory(Path.Combine(proj3, ".cybuild"));

        var app = new CommandAppTester();
        app.SetDefaultCommand<CleanCommand>();

        // Act
        var result = app.Run(temp, "-y");

        Assert.Equal(0, result.ExitCode);
        Assert.False(Directory.Exists(proj1));
        Assert.False(Directory.Exists(proj2));
        Assert.True(Directory.Exists(proj3));

        Directory.Delete(temp, true);
    }
}
