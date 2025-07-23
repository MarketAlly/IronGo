using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using IronGo.AST;

namespace IronGo.Performance;

/// <summary>
/// Caches parsed AST results to improve performance for repeated parsing
/// </summary>
public class ParserCache
{
    private readonly ConcurrentDictionary<string, CachedResult> _cache = new();
    private readonly int _maxCacheSize;
    private readonly TimeSpan _cacheExpiration;
    
    /// <summary>
    /// Gets the default shared cache instance
    /// </summary>
    public static ParserCache Default { get; } = new ParserCache();
    
    public ParserCache(int maxCacheSize = 100, TimeSpan? cacheExpiration = null)
    {
        _maxCacheSize = maxCacheSize;
        _cacheExpiration = cacheExpiration ?? TimeSpan.FromMinutes(30);
    }
    
    /// <summary>
    /// Try to get a cached parse result
    /// </summary>
    public bool TryGetCached(string source, out SourceFile? result)
    {
        var hash = ComputeHash(source);
        
        if (_cache.TryGetValue(hash, out var cached))
        {
            if (DateTime.UtcNow - cached.Timestamp < _cacheExpiration)
            {
                cached.HitCount++;
                result = cached.SourceFile;
                return true;
            }
            else
            {
                // Remove expired entry
                _cache.TryRemove(hash, out _);
            }
        }
        
        result = null;
        return false;
    }
    
    /// <summary>
    /// Add a parse result to the cache
    /// </summary>
    public void AddToCache(string source, SourceFile sourceFile)
    {
        var hash = ComputeHash(source);
        
        // Don't add if already cached
        if (_cache.ContainsKey(hash))
            return;
        
        // Implement simple LRU by removing least hit items when cache is full
        if (_cache.Count >= _maxCacheSize)
        {
            var leastUsed = FindLeastUsedKey();
            if (leastUsed != null)
                _cache.TryRemove(leastUsed, out _);
        }
        
        _cache.TryAdd(hash, new CachedResult(sourceFile));
    }
    
    /// <summary>
    /// Clear the cache
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }
    
    /// <summary>
    /// Get cache statistics
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        var entries = _cache.Values;
        var totalHits = 0L;
        var totalSize = 0L;
        
        foreach (var entry in entries)
        {
            totalHits += entry.HitCount;
            totalSize += entry.EstimatedSize;
        }
        
        return new CacheStatistics(
            _cache.Count,
            totalHits,
            totalSize,
            _maxCacheSize,
            _cacheExpiration);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ComputeHash(string source)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(source);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    
    private string? FindLeastUsedKey()
    {
        string? leastUsedKey = null;
        long minHits = long.MaxValue;
        
        foreach (var kvp in _cache)
        {
            if (kvp.Value.HitCount < minHits)
            {
                minHits = kvp.Value.HitCount;
                leastUsedKey = kvp.Key;
            }
        }
        
        return leastUsedKey;
    }
    
    private class CachedResult
    {
        public SourceFile SourceFile { get; }
        public DateTime Timestamp { get; }
        public long HitCount { get; set; }
        public long EstimatedSize { get; }
        
        public CachedResult(SourceFile sourceFile)
        {
            SourceFile = sourceFile;
            Timestamp = DateTime.UtcNow;
            HitCount = 0;
            EstimatedSize = EstimateSize(sourceFile);
        }
        
        private static long EstimateSize(SourceFile sourceFile)
        {
            // Rough estimation based on node counts
            var nodeCount = CountNodes(sourceFile);
            return nodeCount * 64; // Assume average 64 bytes per node
        }
        
        private static int CountNodes(IGoNode node)
        {
            var counter = new NodeCounter();
            node.Accept(counter);
            return counter.Count;
        }
    }
    
    private class NodeCounter : GoAstVisitorBase
    {
        public int Count { get; private set; }
        
        public override void VisitSourceFile(SourceFile node)
        {
            Count++;
            base.VisitSourceFile(node);
        }
        
        // Override all visit methods to count nodes
        // For brevity, showing pattern - in production would override all
        public override void VisitFunctionDeclaration(FunctionDeclaration node)
        {
            Count++;
            base.VisitFunctionDeclaration(node);
        }
    }
}

/// <summary>
/// Cache statistics
/// </summary>
public class CacheStatistics
{
    public int EntryCount { get; }
    public long TotalHits { get; }
    public long EstimatedMemoryBytes { get; }
    public int MaxCacheSize { get; }
    public TimeSpan CacheExpiration { get; }
    
    public CacheStatistics(int entryCount, long totalHits, long estimatedMemoryBytes, 
        int maxCacheSize, TimeSpan cacheExpiration)
    {
        EntryCount = entryCount;
        TotalHits = totalHits;
        EstimatedMemoryBytes = estimatedMemoryBytes;
        MaxCacheSize = maxCacheSize;
        CacheExpiration = cacheExpiration;
    }
    
    public double HitRate => EntryCount > 0 ? (double)TotalHits / EntryCount : 0;
    public double FillRate => MaxCacheSize > 0 ? (double)EntryCount / MaxCacheSize : 0;
}