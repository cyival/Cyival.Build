using Cyival.Build.Configuration;

namespace Cyival.Build.Plugin.Bundled.Configuration;

public class GitTargetLocationProvider : ITargetLocationProvider
{
    public Type ProvidedType => typeof(GitTargetLocation);

    public string KeyNameOfProvidedType => "git";

    public bool CanProvide(object locationObject)
    {
        if (locationObject is string ls)
            return Uri.IsWellFormedUriString(ls, UriKind.RelativeOrAbsolute);

        if (locationObject is Dictionary<string, object> ld)
            return ld.ContainsKey("url") && ld["url"] is string;

        return false;
    }

    public ITargetLocation Parse(object locationObject, string manifestDir)
    {
        throw new NotImplementedException();
    }
}
