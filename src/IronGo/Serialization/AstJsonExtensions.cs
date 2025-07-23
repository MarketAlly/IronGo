using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MarketAlly.IronGo.AST;

namespace MarketAlly.IronGo.Serialization;

/// <summary>
/// Extension methods for JSON serialization of AST nodes
/// </summary>
public static class AstJsonExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = CreateDefaultOptions();
    
    /// <summary>
    /// Creates default JSON serializer options for AST serialization
    /// </summary>
    public static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new AstJsonConverter() }
        };
        
        return options;
    }
    
    /// <summary>
    /// Converts an AST node to JSON string
    /// </summary>
    public static string ToJson(this IGoNode node, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(node, options ?? DefaultOptions);
    }
    
    /// <summary>
    /// Converts an AST node to pretty-printed JSON string
    /// </summary>
    public static string ToJsonPretty(this IGoNode node)
    {
        var options = CreateDefaultOptions();
        options.WriteIndented = true;
        return JsonSerializer.Serialize(node, options);
    }
    
    /// <summary>
    /// Converts an AST node to compact JSON string
    /// </summary>
    public static string ToJsonCompact(this IGoNode node)
    {
        var options = CreateDefaultOptions();
        options.WriteIndented = false;
        return JsonSerializer.Serialize(node, options);
    }
    
    /// <summary>
    /// Writes an AST node to a stream as JSON
    /// </summary>
    public static void WriteJson(this IGoNode node, Stream stream, JsonSerializerOptions? options = null)
    {
        using var writer = new Utf8JsonWriter(stream);
        JsonSerializer.Serialize(writer, node, options ?? DefaultOptions);
    }
    
    /// <summary>
    /// Writes an AST node to a stream as JSON asynchronously
    /// </summary>
    public static async Task WriteJsonAsync(this IGoNode node, Stream stream, JsonSerializerOptions? options = null)
    {
        await JsonSerializer.SerializeAsync(stream, node, options ?? DefaultOptions);
    }
    
    /// <summary>
    /// Writes an AST node to a file as JSON
    /// </summary>
    public static void WriteJsonToFile(this IGoNode node, string filePath, JsonSerializerOptions? options = null)
    {
        using var stream = File.Create(filePath);
        node.WriteJson(stream, options);
    }
    
    /// <summary>
    /// Writes an AST node to a file as JSON asynchronously
    /// </summary>
    public static async Task WriteJsonToFileAsync(this IGoNode node, string filePath, JsonSerializerOptions? options = null)
    {
        await using var stream = File.Create(filePath);
        await node.WriteJsonAsync(stream, options);
    }
}