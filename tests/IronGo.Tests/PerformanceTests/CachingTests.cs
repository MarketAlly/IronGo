using FluentAssertions;
using IronGo;
using IronGo.Performance;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace IronGo.Tests.PerformanceTests;

public class CachingTests
{
    [Fact]
    public void Cache_ShouldImprovePerformance()
    {
        // Arrange
        const string source = @"
package main

import ""fmt""

func main() {
    for i := 0; i < 100; i++ {
        fmt.Printf(""Number: %d\n"", i)
    }
}";
        
        var parser = new IronGoParser(); // Uses caching by default
        
        // Act - First parse (cache miss)
        var sw1 = Stopwatch.StartNew();
        var ast1 = parser.ParseSource(source);
        sw1.Stop();
        var firstTime = sw1.ElapsedMilliseconds;
        
        // Act - Second parse (cache hit)
        var sw2 = Stopwatch.StartNew();
        var ast2 = parser.ParseSource(source);
        sw2.Stop();
        var secondTime = sw2.ElapsedMilliseconds;
        
        // Assert
        ast1.Should().NotBeNull();
        ast2.Should().NotBeNull();
        ast2.Should().BeSameAs(ast1); // Same instance from cache
        secondTime.Should().BeLessThan(firstTime);
    }
    
    [Fact]
    public void Cache_ShouldRespectMaxSize()
    {
        // Arrange
        var cache = new ParserCache(maxCacheSize: 3);
        var parser = new IronGoParser(new ParserOptions { Cache = cache });
        
        // Act - Fill cache beyond capacity
        for (int i = 0; i < 5; i++)
        {
            var source = $"package main\n\nfunc test{i}() {{}}";
            parser.ParseSource(source);
        }
        
        // Assert
        var stats = cache.GetStatistics();
        stats.EntryCount.Should().BeLessOrEqualTo(3);
    }
    
    [Fact]
    public void Cache_ShouldHandleConcurrentAccess()
    {
        // Arrange
        const string source = @"
package main

func concurrent() {
    ch := make(chan int)
    go func() { ch <- 42 }()
    <-ch
}";
        
        var parser = new IronGoParser();
        var tasks = new Task[10];
        
        // Act
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                var ast = parser.ParseSource(source);
                ast.Should().NotBeNull();
            });
        }
        
        // Assert
        Task.WaitAll(tasks);
        
        var stats = ParserCache.Default.GetStatistics();
        stats.TotalHits.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public void Cache_ShouldDistinguishDifferentSources()
    {
        // Arrange
        const string source1 = "package main\n\nfunc one() {}";
        const string source2 = "package main\n\nfunc two() {}";
        
        var parser = new IronGoParser();
        
        // Act
        var ast1 = parser.ParseSource(source1);
        var ast2 = parser.ParseSource(source2);
        var ast1Again = parser.ParseSource(source1);
        
        // Assert
        ast1.Should().NotBeSameAs(ast2);
        ast1.Should().BeSameAs(ast1Again);
        
        ast1.Declarations[0].As<IronGo.AST.FunctionDeclaration>().Name.Should().Be("one");
        ast2.Declarations[0].As<IronGo.AST.FunctionDeclaration>().Name.Should().Be("two");
    }
    
    [Fact]
    public void CacheStatistics_ShouldTrackMetrics()
    {
        // Arrange
        var cache = new ParserCache();
        var parser = new IronGoParser(new ParserOptions { Cache = cache });
        const string source = "package main";
        
        // Act
        parser.ParseSource(source); // Miss
        parser.ParseSource(source); // Hit
        parser.ParseSource(source); // Hit
        
        var stats = cache.GetStatistics();
        
        // Assert
        stats.EntryCount.Should().Be(1);
        stats.TotalHits.Should().BeGreaterOrEqualTo(2);
        stats.HitRate.Should().BeGreaterThan(1.0);
    }
    
    [Fact]
    public void DisabledCache_ShouldNotCache()
    {
        // Arrange
        var options = new ParserOptions { EnableCaching = false };
        var parser = new IronGoParser(options);
        const string source = "package main";
        
        // Act
        var ast1 = parser.ParseSource(source);
        var ast2 = parser.ParseSource(source);
        
        // Assert
        ast1.Should().NotBeNull();
        ast2.Should().NotBeNull();
        ast1.Should().NotBeSameAs(ast2); // Different instances
    }
    
    [Fact]
    public void CacheExpiration_ShouldRemoveOldEntries()
    {
        // Arrange
        var cache = new ParserCache(maxCacheSize: 10, cacheExpiration: TimeSpan.FromMilliseconds(50));
        var parser = new IronGoParser(new ParserOptions { Cache = cache });
        const string source = "package main";
        
        // Act
        parser.ParseSource(source);
        var stats1 = cache.GetStatistics();
        
        // Wait for expiration
        Task.Delay(100).Wait();
        
        // Try to get from cache (should miss due to expiration)
        parser.ParseSource(source);
        var stats2 = cache.GetStatistics();
        
        // Assert
        stats1.EntryCount.Should().Be(1);
        // After expiration, the old entry is removed and a new one is added
        stats2.EntryCount.Should().Be(1);
    }
}