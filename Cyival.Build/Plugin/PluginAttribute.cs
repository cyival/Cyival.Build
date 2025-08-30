namespace Cyival.Build.Plugin;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PluginAttribute(string id) : Attribute
{
    public string Id { get; } = id;
}