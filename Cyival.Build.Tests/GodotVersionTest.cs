﻿namespace Cyival.Build.Tests;

using Xunit;
using Environment;

public class GodotVersionTests
{
    [Theory]
    [InlineData("4.2", 4, 2, 0, GodotChannel.Stable, 0)]
    [InlineData("4.4.0", 4, 4, 0, GodotChannel.Stable, 0)]
    [InlineData("4.4.stable", 4, 4, 0, GodotChannel.Stable, 0)]
    [InlineData("4.4.dev2", 4, 4, 0, GodotChannel.Dev, 2)]
    [InlineData("4.4.1.stable.mono.official.49a5bc7b6", 4, 4, 1, GodotChannel.Stable, 0)]
    [InlineData("v4.4.1.stable", 4, 4, 1, GodotChannel.Stable, 0)]
    [InlineData("3.5.2.rc1", 3, 5, 2, GodotChannel.ReleaseCandidate, 1)]
    [InlineData("4.0.beta4", 4, 0, 0, GodotChannel.Beta, 4)]
    [InlineData("4.4-dev2", 4, 4, 0, GodotChannel.Dev, 2)] // This should work now
    [InlineData("4.4-stable", 4, 4, 0, GodotChannel.Stable, 0)] // This should work now
    [InlineData("4.4.1-stable", 4, 4, 1, GodotChannel.Stable, 0)] // This should work now
    public void Parse_ValidVersionStrings_ReturnsCorrectGodotVersion(
        string versionString, 
        int expectedMajor, 
        int expectedMinor, 
        int expectedPatch, 
        GodotChannel expectedChannel, 
        int expectedStatus)
    {
        // Act
        var result = GodotVersion.Parse(versionString);

        // Assert
        Assert.Equal(expectedMajor, result.Major);
        Assert.Equal(expectedMinor, result.Minor);
        Assert.Equal(expectedPatch, result.Patch);
        Assert.Equal(expectedChannel, result.Channel);
        Assert.Equal(expectedStatus, result.StatusVersion);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("4")]
    [InlineData("4.x")]
    [InlineData("4.4.invalid")]
    public void Parse_InvalidVersionStrings_ThrowsException(string? versionString)
    {
        // Act & Assert
        #pragma warning disable CS8604
        Assert.ThrowsAny<Exception>(() => GodotVersion.Parse(versionString));
        #pragma warning restore CS8604
    }

    [Fact]
    public void Parse_VersionWithMultipleChannelMarkers_UsesFirstValidChannel()
    {
        // This test ensures that if there are multiple channel markers,
        // the parser uses the first valid one it encounters
        var versionString = "4.4.1.dev3.stable.rc2";
        
        // Act
        var result = GodotVersion.Parse(versionString);
        
        // Assert - should use "dev3" (first valid channel) and ignore the others
        Assert.Equal(GodotChannel.Dev, result.Channel);
        Assert.Equal(3, result.StatusVersion);
    }

    [Fact]
    public void Parse_VersionWithGitHash_IgnoresGitHash()
    {
        // This test ensures that git hashes are properly ignored
        var versionString = "4.4.1.stable.mono.official.49a5bc7b6";
        
        // Act
        var result = GodotVersion.Parse(versionString);
        
        // Assert
        Assert.Equal(4, result.Major);
        Assert.Equal(4, result.Minor);
        Assert.Equal(1, result.Patch);
        Assert.Equal(GodotChannel.Stable, result.Channel);
        Assert.Equal(0, result.StatusVersion);
    }
}