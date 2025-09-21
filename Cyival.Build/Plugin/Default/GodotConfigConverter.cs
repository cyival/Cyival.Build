using Tomlyn.Model;

namespace Cyival.Build.Plugin.Default;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Tomlyn;
using Tomlyn.Model;

public class GodotConfigConverter
{
    /// <summary>
    /// Converts and reads a Godot CFG file to TOML format
    /// </summary>
    /// <param name="cfgFilePath">Path to the Godot CFG file</param>
    /// <returns>Dictionary containing the configuration data</returns>
    public static Dictionary<string, object> ConvertAndReadCfgToToml(string cfgFilePath)
    {
        // Read CFG file content
        var cfgContent = File.ReadAllText(cfgFilePath);
        
        // Convert to TOML format
        var tomlContent = ConvertCfgToToml(cfgContent);
        
        // Parse TOML content
        return ParseTomlContent(tomlContent);
    }
    
    /// <summary>
    /// Converts Godot CFG format to TOML format
    /// </summary>
    /// <param name="cfgContent">CFG file content</param>
    /// <returns>TOML formatted string</returns>
    public static string ConvertCfgToToml(string cfgContent)
    {
        var tomlBuilder = new StringBuilder();

        using (var reader = new StringReader(cfgContent))
        {
            while (reader.ReadLine() is { } line)
            {
                line = line.Trim();
                
                // Skip empty lines and comments
                if (string.IsNullOrEmpty(line) || line.StartsWith(';'))
                    continue;
                
                // Handle sections
                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    var currentSection = line.Substring(1, line.Length - 2);
                    tomlBuilder.AppendLine($"[{currentSection}]");
                    continue;
                }
                
                // Handle key-value pairs
                var equalsIndex = line.IndexOf('=');
                if (equalsIndex <= 0) continue;
                
                var key = line[..equalsIndex].Trim();
                var value = line[(equalsIndex + 1)..].Trim();
                    
                // Handle Godot special value types
                value = ConvertGodotValueToToml(value);
                    
                tomlBuilder.AppendLine($"{key} = {value}");
            }
        }
        
        return tomlBuilder.ToString();
    }
    
    /// <summary>
    /// Converts Godot special values to TOML compatible format
    /// Compatible with both Godot 3.x and 4.x array formats
    /// </summary>
    public static string ConvertGodotValueToToml(string value)
    {
        // Handle Godot 3.x PoolStringArray and Godot 4.x PackedStringArray formats
        if ((value.StartsWith("PoolStringArray") || value.StartsWith("PackedStringArray")) && 
            value.Contains('['))
        {
            // Extract array content
            var start = value.IndexOf('[') + 1;
            var end = value.LastIndexOf(']');
            if (start < end)
            {
                var arrayContent = value.Substring(start, end - start);
                // Convert to TOML array format
                return $"[{arrayContent}]";
            }
        }
        
        // Handle Godot 3.x PoolIntArray and Godot 4.x PackedInt32Array formats
        if ((value.StartsWith("PoolIntArray") || value.StartsWith("PackedInt32Array")) && 
            value.Contains('['))
        {
            var start = value.IndexOf('[') + 1;
            var end = value.LastIndexOf(']');
            if (start < end)
            {
                var arrayContent = value.Substring(start, end - start);
                return $"[{arrayContent}]";
            }
        }
        
        // Handle Godot 3.x PoolRealArray and Godot 4.x PackedFloat32Array formats
        if ((value.StartsWith("PoolRealArray") || value.StartsWith("PackedFloat32Array")) && 
            value.Contains('['))
        {
            var start = value.IndexOf('[') + 1;
            var end = value.LastIndexOf(']');
            if (start < end)
            {
                var arrayContent = value.Substring(start, end - start);
                return $"[{arrayContent}]";
            }
        }
        
        // Handle Godot 3.x PoolVector2Array and Godot 4.x PackedVector2Array formats
        if ((value.StartsWith("PoolVector2Array") || value.StartsWith("PackedVector2Array")) && 
            value.Contains('['))
        {
            var start = value.IndexOf('[') + 1;
            var end = value.LastIndexOf(']');
            if (start < end)
            {
                var arrayContent = value.Substring(start, end - start);
                // Convert Vector2 format to TOML compatible arrays
                arrayContent = Regex.Replace(arrayContent, @"Vector2\(([^)]+)\)", "[$1]");
                return $"[{arrayContent}]";
            }
        }
        
        // Handle boolean values
        if (value.ToLower() == "true" || value.ToLower() == "false")
        {
            return value.ToLower();
        }
        
        // Handle numbers
        if (double.TryParse(value, out _))
        {
            return value;
        }
        
        // Handle already quoted strings
        if ((value.StartsWith('"') && value.EndsWith('"')) || 
            (value.StartsWith('\'') && value.EndsWith('\'')))
        {
            return value;
        }
        
        // Handle regular arrays (not Godot-specific)
        if (value.StartsWith('[') && value.EndsWith(']'))
        {
            return value;
        }
        
        // Other cases: treat as string and add quotes
        return $"\"{value}\"";
    }
    
    /// <summary>
    /// Parses TOML content
    /// </summary>
    public static Dictionary<string, object> ParseTomlContent(string tomlContent)
    {
        try
        {
            // Parse TOML content
            var model = Toml.ToModel(tomlContent);
            
            // Convert to nested dictionary for easier access
            return ConvertToNestedDictionary(model);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error parsing TOML content: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Converts TomlTable to nested dictionary
    /// </summary>
    private static Dictionary<string, object> ConvertToNestedDictionary(TomlTable table)
    {
        var result = new Dictionary<string, object>();
        
        foreach (var key in table.Keys)
        {
            var value = table[key];
            
            if (value is TomlTable subTable)
            {
                result[key] = ConvertToNestedDictionary(subTable);
            }
            else if (value is TomlArray array)
            {
                result[key] = ConvertTomlArray(array);
            }
            else
            {
                result[key] = value;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Converts TomlArray to .NET list
    /// </summary>
    private static List<object> ConvertTomlArray(TomlArray array)
    {
        var result = new List<object>();
        
        foreach (var item in array)
        {
            if (item is TomlTable subTable)
            {
                result.Add(ConvertToNestedDictionary(subTable));
            }
            else if (item is TomlArray subArray)
            {
                result.Add(ConvertTomlArray(subArray));
            }
            else
            {
                result.Add(item);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Gets Godot version-specific array type names
    /// </summary>
    /// <param name="isGodot4">Whether it's Godot 4.x</param>
    /// <param name="arrayType">Array type (string, int, float, vector2)</param>
    /// <returns>Version-specific array type name</returns>
    public static string GetGodotArrayTypeName(bool isGodot4, string arrayType)
    {
        if (isGodot4)
        {
            return arrayType switch
            {
                "string" => "PackedStringArray",
                "int" => "PackedInt32Array",
                "float" => "PackedFloat32Array",
                "vector2" => "PackedVector2Array",
                _ => "PackedStringArray"
            };
        }
        else
        {
            return arrayType switch
            {
                "string" => "PoolStringArray",
                "int" => "PoolIntArray",
                "float" => "PoolRealArray",
                "vector2" => "PoolVector2Array",
                _ => "PoolStringArray"
            };
        }
    }
}