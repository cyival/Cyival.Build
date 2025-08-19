namespace Cyival.Build;

/// <summary>
/// Marks to be loaded.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class BuildPluginAttribute : Attribute
{
    public BuildPluginAttribute()
    {
        // Nothing
    }
}