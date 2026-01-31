using System.Threading;
using Cyival.Build.Cli.Command;
using Spectre.Console.Cli;

namespace Cyival.Build.Tests;

public class CleanCommandTests
{
    [Fact]
    public void Execute_NoManifest_ShouldAbortAndNotDelete()
    {
        var temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temp);

        var proj = Path.Combine(temp, "project");
        Directory.CreateDirectory(proj);
        Directory.CreateDirectory(Path.Combine(proj, ".cybuild"));

        var settings = new CleanCommand.Settings { Path = temp, AgreeAll = true };
        var cmd = new CleanCommand();

        var rc = cmd.Execute(default(CommandContext), settings, CancellationToken.None);

        Assert.Equal(1, rc);
        Assert.True(Directory.Exists(Path.Combine(proj, ".cybuild")));

        Directory.Delete(temp, true);
    }

    [Fact]
    public void Execute_WithManifest_DeletesUpToDepthTwoOnly()
    {
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

        var settings = new CleanCommand.Settings { Path = temp, AgreeAll = true };
        var cmd = new CleanCommand();

        var rc = cmd.Execute(default(CommandContext), settings, CancellationToken.None);

        Assert.Equal(0, rc);
        Assert.False(Directory.Exists(proj1));
        Assert.False(Directory.Exists(proj2));
        Assert.True(Directory.Exists(proj3));

        Directory.Delete(temp, true);
    }
}