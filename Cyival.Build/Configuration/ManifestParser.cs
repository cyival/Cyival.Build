using Tomlyn;

namespace Cyival.Build.Configuration;

public static class ManifestParser
{
    public static BuildManifest Parse(string manifestPath)
    {
        if (!File.Exists(manifestPath))
        {
             throw new ArgumentException("File not exists.", nameof(manifestPath));
        }

        var manifestString = File.ReadAllText(manifestPath);
        var model = Toml.ToModel(manifestString);

        return new BuildManifest();
    }
}