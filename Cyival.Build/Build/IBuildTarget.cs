using Tomlyn.Model;

namespace Cyival.Build.Build;

public interface IBuildTarget
{
    public bool IsDefault { get; set; }
    
    public string Path { get; }
    
    public string Id { get; }
    
    public List<string> Requirements { get; }
    
    public void SetLocalConfiguration<T>(T configuration);

    public T? GetLocalConfiguration<T>();
    
    /// <summary>
    /// A custom parser for TOML table to parse local configuration.
    /// </summary>
    /// <param name="table">TOML table</param>
    public void ParseFromTable(TomlTable table) { }
}