using Cyival.Build.Cli.Command;
using Spectre.Console.Cli.Testing;

public class BuildCommandManifestTests
{
    [Fact]
    public void Execute_NoManifest_ShouldAbort()
    {
        // Arrange
        var app = new CommandAppTester();
        app.SetDefaultCommand<BuildCommand>();

        // Act
        var result = app.Run();

        // Assert
        Assert.Equal(-1, result.ExitCode);
    }

    [Fact]
    public void Execute_EmptyManifest_ShouldAbort()
    {
        // Arrange
        var temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temp);

        // place manifest in root
        File.WriteAllText(Path.Combine(temp, "build.toml"), "minimal-version = 0.1\n");

        var app = new CommandAppTester();
        app.SetDefaultCommand<BuildCommand>();

        // Act
        var result = app.Run(temp);

        // Assert
        Assert.Equal(-1, result.ExitCode);
    }

    [Fact]
    public void Execute_InvalidManifest_ShouldAbort()
    {
        // Arrange
        var temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temp);

        // place manifest in root
        File.WriteAllText(Path.Combine(temp, "build.toml"), "this is invalid\n");

        var app = new CommandAppTester();
        app.SetDefaultCommand<BuildCommand>();

        // Act
        var result = app.Run(temp);

        // Assert
        Assert.Equal(-1, result.ExitCode);
    }
}
