namespace Cyival.Build;

public class PathSolver
{
    private readonly string _basePath;

    public PathSolver(string basePath)
    {
        if (File.Exists(basePath))
        {
            basePath = Path.Combine(basePath, "..");
        }

        _basePath = Path.GetFullPath(basePath);
    }

    public string GetPathTo(params string[] relativePath)
    {
        var rp = Path.Combine([_basePath, .. relativePath]);
        return Path.GetFullPath(rp);
    }
}