using Tomlyn.Model;

namespace Cyival.Build.Build;

public interface IBuildTarget
{
    public bool IsDefault { get; set; }
    
    public string SourcePath { get; }
    
    public string DestinationPath { get; init; }
    
    public string Id { get; }
    
    public List<string> Requirements { get; }
    
    public void SetLocalConfiguration<T>(T configuration);

    public T? GetLocalConfiguration<T>();
    
    public bool TryGetLocalConfiguration<T>(out T? configuration);
    
    /// <summary>
    /// A custom parser for TOML table to parse local configuration.
    /// </summary>
    /// <param name="table">TOML table</param>
    public void ParseFromTable(TomlTable table) { }
}