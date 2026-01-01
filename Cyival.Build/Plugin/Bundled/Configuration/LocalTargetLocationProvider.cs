namespace Cyival.Build.Plugin.Bundled;

using System;
using Configuration;

public class LocalTargetLocationProvider : ITargetLocationProvider
{
    public Type ProvidedType => typeof(LocalTargetLocation);

    public string KeyNameOfProvidedType => "path";

    public bool CanProvide(object locationObject) => locationObject is string;

    public ITargetLocation Parse(object locationObject, string manifestDir)
    {
        var relSourcePath = (string)locationObject;

        if (string.IsNullOrWhiteSpace(relSourcePath))
            throw new NotSupportedException($"Target does not have a valid source path.");

        var srcPath = new PathSolver(manifestDir).GetPathTo(relSourcePath);

        return new LocalTargetLocation(relSourcePath, srcPath);
    }
}
