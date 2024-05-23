using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

public class HackerNewsServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly IMemoryCache _cache;
    private readonly HackerNewsService _service;

    public HackerNewsServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _cache = new MemoryCache(new MemoryCacheOptions());

        var httpClient = new HttpClient(new MockHttpMessageHandler());
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _service = new HackerNewsService(_httpClientFactoryMock.Object, _cache);
    }

    [Fact]
    public async Task GetNewStoriesAsync_ReturnsStories()
    {
        // Arrange
        var expectedStoryIds = new List<int> { 1, 2, 3 };

        // Add expectedStoryIds to cache to simulate cache hit
        _cache.Set("newstories", expectedStoryIds);

        // Act
        var result = await _service.GetNewStoriesAsync(1, 3);

        // Assert
        Assert.Equal(3, result.total);
        Assert.NotEmpty(result.newStories);
    }

    // You can add more tests to cover other scenarios
}

// Mock HttpMessageHandler to simulate HttpClient responses
public class MockHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("[1, 2, 3]")
        };

        if (request.RequestUri.ToString().Contains("item"))
        {
            response.Content = new StringContent("{\"id\": 1, \"title\": \"Test Story\"}");
        }

        return Task.FromResult(response);
    }
}
