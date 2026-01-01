using Cyival.Build.Configuration;

namespace Cyival.Build.Plugin.Bundled.Configuration;

public class GitTargetLocationProvider : ITargetLocationProvider
{
    public Type ProvidedType => typeof(GitTargetLocation);

    public string KeyNameOfProvidedType => "git";

    public bool CanProvide(object locationObject)
    {
        if (locationObject is string)
            return true;

        if (locationObject is Dictionary<string, object> ld)
            return ld.ContainsKey("url") && ld["url"] is string;

        return false;
    }

    public ITargetLocation Parse(object locationObject, string manifestDir)
    {
        GitRepository? repo = null;
        if (locationObject is string locString)
        {
            repo = new GitRepository
            {
                Repository = locString,
            };
        }
        else if (locationObject is Dictionary<string, object?> locData)
        {
            repo = new GitRepository
            {
                Repository = (string)(locData["url"] ?? ""),
                Branch = (string?)locData.GetValueOrDefault("branch", null),
            };
        }

        if (repo is null)
        {
            throw new InvalidDataException();
        }

        return new GitTargetLocation(repo.Value);
    }
}
