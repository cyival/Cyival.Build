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
        var baseRp = Path.Combine(relativePath);

        if (Path.IsPathRooted(baseRp))
            return baseRp;
        
        var rp = Path.Combine(_basePath, baseRp);
        return Path.GetFullPath(rp);
    }

    public string GetBasePath() => _basePath;
    
    public PathSolver GetSubSolver(params string[] relativePath)
        => new(GetPathTo(relativePath));
}