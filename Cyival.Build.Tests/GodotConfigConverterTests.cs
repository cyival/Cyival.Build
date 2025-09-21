using Cyival.Build.Plugin.Default;

namespace Cyival.Build.Tests;


public class GodotConfigConverterTests
{
    [Fact]
    public void ConvertAndReadCfgToToml_WithSimpleConfig_ReturnsCorrectData()
    {
        // Create temporary CFG file
        string cfgContent = @"
[player]
name = ""John""
health = 100
score = 500.5
is_alive = true

[graphics]
fullscreen = false
resolution = [1920, 1080]
";
        
        string tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, cfgContent);
        
        try
        {
            // Execute conversion
            var result = GodotConfigConverter.ConvertAndReadCfgToToml(tempFilePath);
            
            // Verify results
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("player"));
            Assert.True(result.ContainsKey("graphics"));
            
            var playerSection = result["player"] as Dictionary<string, object>;
            Assert.NotNull(playerSection);
            Assert.Equal("John", playerSection["name"]);
            Assert.Equal(100L, playerSection["health"]); // Tomlyn parses integers as long
            Assert.Equal(500.5, playerSection["score"]);
            Assert.True((bool)playerSection["is_alive"]);
            
            var graphicsSection = result["graphics"] as Dictionary<string, object>;
            Assert.NotNull(graphicsSection);
            Assert.False((bool)graphicsSection["fullscreen"]);
            
            var resolution = graphicsSection["resolution"] as List<object>;
            Assert.NotNull(resolution);
            Assert.Equal(2, resolution.Count);
            Assert.Equal(1920L, resolution[0]);
            Assert.Equal(1080L, resolution[1]);
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
    
    [Fact]
    public void ConvertAndReadCfgToToml_WithGodot3Arrays_ReturnsCorrectData()
    {
        // Create temporary CFG file (Godot 3.x format)
        string cfgContent = @"
[inventory]
items = PoolStringArray[""sword"", ""shield"", ""potion""]
quantities = PoolIntArray[1, 5, 3]
positions = PoolVector2Array[Vector2(10, 20), Vector2(30, 40)]
";
        
        string tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, cfgContent);
        
        try
        {
            // Execute conversion
            var result = GodotConfigConverter.ConvertAndReadCfgToToml(tempFilePath);
            
            // Verify results
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("inventory"));
            
            var inventorySection = result["inventory"] as Dictionary<string, object>;
            Assert.NotNull(inventorySection);
            
            // Verify string array
            var items = inventorySection["items"] as List<object>;
            Assert.NotNull(items);
            Assert.Equal(3, items.Count);
            Assert.Equal("sword", items[0]);
            Assert.Equal("shield", items[1]);
            Assert.Equal("potion", items[2]);
            
            // Verify integer array
            var quantities = inventorySection["quantities"] as List<object>;
            Assert.NotNull(quantities);
            Assert.Equal(3, quantities.Count);
            Assert.Equal(1L, quantities[0]);
            Assert.Equal(5L, quantities[1]);
            Assert.Equal(3L, quantities[2]);
            
            // Verify Vector2 array
            var positions = inventorySection["positions"] as List<object>;
            Assert.NotNull(positions);
            Assert.Equal(2, positions.Count);
            
            var pos1 = positions[0] as List<object>;
            Assert.NotNull(pos1);
            Assert.Equal(10L, pos1[0]);
            Assert.Equal(20L, pos1[1]);
            
            var pos2 = positions[1] as List<object>;
            Assert.NotNull(pos2);
            Assert.Equal(30L, pos2[0]);
            Assert.Equal(40L, pos2[1]);
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
    
    [Fact]
    public void ConvertAndReadCfgToToml_WithGodot4Arrays_ReturnsCorrectData()
    {
        // Create temporary CFG file (Godot 4.x format)
        string cfgContent = @"
[inventory]
items = PackedStringArray[""sword"", ""shield"", ""potion""]
quantities = PackedInt32Array[1, 5, 3]
positions = PackedVector2Array[Vector2(10, 20), Vector2(30, 40)]
";
        
        string tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, cfgContent);
        
        try
        {
            // Execute conversion
            var result = GodotConfigConverter.ConvertAndReadCfgToToml(tempFilePath);
            
            // Verify results
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("inventory"));
            
            var inventorySection = result["inventory"] as Dictionary<string, object>;
            Assert.NotNull(inventorySection);
            
            // Verify string array
            var items = inventorySection["items"] as List<object>;
            Assert.NotNull(items);
            Assert.Equal(3, items.Count);
            Assert.Equal("sword", items[0]);
            Assert.Equal("shield", items[1]);
            Assert.Equal("potion", items[2]);
            
            // Verify integer array
            var quantities = inventorySection["quantities"] as List<object>;
            Assert.NotNull(quantities);
            Assert.Equal(3, quantities.Count);
            Assert.Equal(1L, quantities[0]);
            Assert.Equal(5L, quantities[1]);
            Assert.Equal(3L, quantities[2]);
            
            // Verify Vector2 array
            var positions = inventorySection["positions"] as List<object>;
            Assert.NotNull(positions);
            Assert.Equal(2, positions.Count);
            
            var pos1 = positions[0] as List<object>;
            Assert.NotNull(pos1);
            Assert.Equal(10L, pos1[0]);
            Assert.Equal(20L, pos1[1]);
            
            var pos2 = positions[1] as List<object>;
            Assert.NotNull(pos2);
            Assert.Equal(30L, pos2[0]);
            Assert.Equal(40L, pos2[1]);
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
    
    [Fact]
    public void ConvertAndReadCfgToToml_WithMixedArrays_ReturnsCorrectData()
    {
        // Create temporary CFG file (mixed Godot 3.x and 4.x formats)
        string cfgContent = @"
[arrays]
strings = PoolStringArray[""a"", ""b""]
ints = PackedInt32Array[1, 2]
floats = PoolRealArray[1.5, 2.5]
vectors = PackedVector2Array[Vector2(1, 2), Vector2(3, 4)]
";
        
        string tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, cfgContent);
        
        try
        {
            // Execute conversion
            var result = GodotConfigConverter.ConvertAndReadCfgToToml(tempFilePath);
            
            // Verify results
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("arrays"));
            
            var arraysSection = result["arrays"] as Dictionary<string, object>;
            Assert.NotNull(arraysSection);
            
            // Verify string array
            var strings = arraysSection["strings"] as List<object>;
            Assert.NotNull(strings);
            Assert.Equal(2, strings.Count);
            Assert.Equal("a", strings[0]);
            Assert.Equal("b", strings[1]);
            
            // Verify integer array
            var ints = arraysSection["ints"] as List<object>;
            Assert.NotNull(ints);
            Assert.Equal(2, ints.Count);
            Assert.Equal(1L, ints[0]);
            Assert.Equal(2L, ints[1]);
            
            // Verify float array
            var floats = arraysSection["floats"] as List<object>;
            Assert.NotNull(floats);
            Assert.Equal(2, floats.Count);
            Assert.Equal(1.5, floats[0]);
            Assert.Equal(2.5, floats[1]);
            
            // Verify Vector2 array
            var vectors = arraysSection["vectors"] as List<object>;
            Assert.NotNull(vectors);
            Assert.Equal(2, vectors.Count);
            
            var vec1 = vectors[0] as List<object>;
            Assert.NotNull(vec1);
            Assert.Equal(1L, vec1[0]);
            Assert.Equal(2L, vec1[1]);
            
            var vec2 = vectors[1] as List<object>;
            Assert.NotNull(vec2);
            Assert.Equal(3L, vec2[0]);
            Assert.Equal(4L, vec2[1]);
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
    
    [Fact]
    public void ConvertAndReadCfgToToml_WithCommentsAndEmptyLines_IgnoresThem()
    {
        // Create temporary CFG file (with comments and empty lines)
        string cfgContent = @"
; This is a comment

[player]
; Player name
name = ""John""

; Player health
health = 100

[graphics]
fullscreen = true
";
        
        string tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, cfgContent);
        
        try
        {
            // Execute conversion
            var result = GodotConfigConverter.ConvertAndReadCfgToToml(tempFilePath);
            
            // Verify results
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("player"));
            Assert.True(result.ContainsKey("graphics"));
            
            var playerSection = result["player"] as Dictionary<string, object>;
            Assert.NotNull(playerSection);
            Assert.Equal("John", playerSection["name"]);
            Assert.Equal(100L, playerSection["health"]);
            
            var graphicsSection = result["graphics"] as Dictionary<string, object>;
            Assert.NotNull(graphicsSection);
            Assert.True((bool)graphicsSection["fullscreen"]);
            
            // Ensure comments and empty lines are not included as keys
            Assert.False(playerSection.ContainsKey("; Player name"));
            Assert.False(playerSection.ContainsKey("; Player health"));
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
    
    [Fact]
    public void ConvertAndReadCfgToToml_WithNonExistentFile_ThrowsException()
    {
        // Execute conversion
        Assert.Throws<FileNotFoundException>(() => 
            GodotConfigConverter.ConvertAndReadCfgToToml("nonexistent.cfg"));
    }
    
    [Fact]
    public void GetGodotArrayTypeName_WithDifferentVersions_ReturnsCorrectNames()
    {
        // Test Godot 3.x
        Assert.Equal("PoolStringArray", GodotConfigConverter.GetGodotArrayTypeName(false, "string"));
        Assert.Equal("PoolIntArray", GodotConfigConverter.GetGodotArrayTypeName(false, "int"));
        Assert.Equal("PoolRealArray", GodotConfigConverter.GetGodotArrayTypeName(false, "float"));
        Assert.Equal("PoolVector2Array", GodotConfigConverter.GetGodotArrayTypeName(false, "vector2"));
        
        // Test Godot 4.x
        Assert.Equal("PackedStringArray", GodotConfigConverter.GetGodotArrayTypeName(true, "string"));
        Assert.Equal("PackedInt32Array", GodotConfigConverter.GetGodotArrayTypeName(true, "int"));
        Assert.Equal("PackedFloat32Array", GodotConfigConverter.GetGodotArrayTypeName(true, "float"));
        Assert.Equal("PackedVector2Array", GodotConfigConverter.GetGodotArrayTypeName(true, "vector2"));
        
        // Test default values
        Assert.Equal("PoolStringArray", GodotConfigConverter.GetGodotArrayTypeName(false, "unknown"));
        Assert.Equal("PackedStringArray", GodotConfigConverter.GetGodotArrayTypeName(true, "unknown"));
    }
    
    [Fact]
    public void ConvertGodotValueToToml_WithVariousValues_ConvertsCorrectly()
    {
        // Test strings
        Assert.Equal("\"hello\"", GodotConfigConverter.ConvertGodotValueToToml("hello"));
        Assert.Equal("\"hello\"", GodotConfigConverter.ConvertGodotValueToToml("\"hello\""));
        
        // Test numbers
        Assert.Equal("123", GodotConfigConverter.ConvertGodotValueToToml("123"));
        Assert.Equal("45.67", GodotConfigConverter.ConvertGodotValueToToml("45.67"));
        
        // Test boolean values
        Assert.Equal("true", GodotConfigConverter.ConvertGodotValueToToml("true"));
        Assert.Equal("false", GodotConfigConverter.ConvertGodotValueToToml("false"));
        Assert.Equal("true", GodotConfigConverter.ConvertGodotValueToToml("True"));
        Assert.Equal("false", GodotConfigConverter.ConvertGodotValueToToml("False"));
        
        // Test Godot 3.x arrays
        Assert.Equal("[\"a\", \"b\"]", GodotConfigConverter.ConvertGodotValueToToml("PoolStringArray[\"a\", \"b\"]"));
        Assert.Equal("[1, 2, 3]", GodotConfigConverter.ConvertGodotValueToToml("PoolIntArray[1, 2, 3]"));
        Assert.Equal("[1.5, 2.5]", GodotConfigConverter.ConvertGodotValueToToml("PoolRealArray[1.5, 2.5]"));
        Assert.Equal("[[10, 20], [30, 40]]", GodotConfigConverter.ConvertGodotValueToToml("PoolVector2Array[Vector2(10, 20), Vector2(30, 40)]"));
        
        // Test Godot 4.x arrays
        Assert.Equal("[\"a\", \"b\"]", GodotConfigConverter.ConvertGodotValueToToml("PackedStringArray[\"a\", \"b\"]"));
        Assert.Equal("[1, 2, 3]", GodotConfigConverter.ConvertGodotValueToToml("PackedInt32Array[1, 2, 3]"));
        Assert.Equal("[1.5, 2.5]", GodotConfigConverter.ConvertGodotValueToToml("PackedFloat32Array[1.5, 2.5]"));
        Assert.Equal("[[10, 20], [30, 40]]", GodotConfigConverter.ConvertGodotValueToToml("PackedVector2Array[Vector2(10, 20), Vector2(30, 40)]"));
    }
}