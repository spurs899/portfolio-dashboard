using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using PortfolioManager.Core.Services;

namespace PortfolioManager.Core.Tests.Services;

public class MemoryCacheWrapperTests
{
    private readonly MemoryCacheWrapper _cacheWrapper;

    public MemoryCacheWrapperTests()
    {
        IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheWrapper = new MemoryCacheWrapper(memoryCache);
    }

    [Fact]
    public void Get_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        var result = _cacheWrapper.Get<string>("non-existent-key");

        result.Should().BeNull();
    }

    [Fact]
    public void Set_AndGet_ShouldStoreAndRetrieveValue()
    {
        const string key = "test-key";
        const string value = "test-value";

        _cacheWrapper.Set(key, value);
        var result = _cacheWrapper.Get<string>(key);

        result.Should().Be(value);
    }

    [Fact]
    public void Set_WithComplexType_ShouldStoreAndRetrieve()
    {
        const string key = "test-key";
        var value = new TestData { Id = 1, Name = "Test" };

        _cacheWrapper.Set(key, value);
        var result = _cacheWrapper.Get<TestData>(key);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public void Set_WithExpiration_ShouldExpireAfterTime()
    {
        const string key = "test-key";
        const string value = "test-value";
        var expiration = TimeSpan.FromMilliseconds(100);

        _cacheWrapper.Set(key, value, expiration);
        var resultBefore = _cacheWrapper.Get<string>(key);

        Thread.Sleep(150);

        var resultAfter = _cacheWrapper.Get<string>(key);

        resultBefore.Should().Be(value);
        resultAfter.Should().BeNull();
    }

    [Fact]
    public void Set_WithoutExpiration_ShouldPersist()
    {
        const string key = "test-key";
        const string value = "test-value";

        _cacheWrapper.Set(key, value);
        
        Thread.Sleep(100);
        
        var result = _cacheWrapper.Get<string>(key);

        result.Should().Be(value);
    }

    [Fact]
    public void Remove_ShouldDeleteCachedValue()
    {
        const string key = "test-key";
        const string value = "test-value";

        _cacheWrapper.Set(key, value);
        var beforeRemove = _cacheWrapper.Get<string>(key);
        
        _cacheWrapper.Remove(key);
        var afterRemove = _cacheWrapper.Get<string>(key);

        beforeRemove.Should().Be(value);
        afterRemove.Should().BeNull();
    }

    [Fact]
    public void Set_ShouldOverwriteExistingValue()
    {
        const string key = "test-key";
        const string value1 = "first-value";
        const string value2 = "second-value";

        _cacheWrapper.Set(key, value1);
        _cacheWrapper.Set(key, value2);
        var result = _cacheWrapper.Get<string>(key);

        result.Should().Be(value2);
    }

    [Fact]
    public void Get_WithWrongType_ShouldReturnDefault()
    {
        const string key = "test-key";
        _cacheWrapper.Set(key, "string-value");

        var result = _cacheWrapper.Get<int>(key);

        result.Should().Be(0);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
