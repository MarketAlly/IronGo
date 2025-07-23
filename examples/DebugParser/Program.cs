using IronGo;
using IronGo.AST;
using IronGo.Serialization;
using System;

try
{
    const string simpleGo = @"
package main

func hello() {
    println(""Hello, World!"")
}";
    
    Console.WriteLine("Parsing: " + simpleGo);
    
    var ast = IronGoParser.Parse(simpleGo);
    
    Console.WriteLine("Success!");
    
    // Test JSON serialization
    var json = ast.ToJson();
    Console.WriteLine("\nJSON output:");
    Console.WriteLine(json.Substring(0, Math.Min(json.Length, 200)) + "...");
    
    // Check what properties are in the JSON
    var doc = System.Text.Json.JsonDocument.Parse(json);
    Console.WriteLine("\nRoot properties:");
    foreach (var prop in doc.RootElement.EnumerateObject())
    {
        Console.WriteLine($"  {prop.Name}: {prop.Value.ValueKind}");
    }
}
catch (ParseException ex)
{
    Console.WriteLine($"Parse Error: {ex.Message}");
    Console.WriteLine($"Errors:");
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"  Line {error.Line}, Col {error.Column}: {error.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}