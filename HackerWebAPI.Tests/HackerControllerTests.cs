using HackerWebAPI;
using HackerWebAPI.Controllers;
using HackerWebAPI.Interface;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class HackerControllerTests
{
    private readonly Mock<IHackerNewsService> _hackerNewsServiceMock;
    private readonly HackerController _controller;

    public HackerControllerTests()
    {
        _hackerNewsServiceMock = new Mock<IHackerNewsService>();
        _controller = new HackerController(_hackerNewsServiceMock.Object);
    }

    [Fact]
    public async Task GetNewStories_ReturnsOkResult_WithStories()
    {
        // Arrange
        var stories = new List<object> { new { id = 1, title = "Test Story" } };
        _hackerNewsServiceMock.Setup(service => service.GetNewStoriesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((1, stories));

        // Act
        var result = await _controller.GetNewStories(1, 1) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.IsType<OkObjectResult>(result);

        var value = result.Value as NewStoriesResponse;
        Assert.NotNull(value);
        Assert.Equal(1, value.Total);
        Assert.Equal(stories, value.NewStories);
    }

    [Fact]
    public async Task GetNewStories_Returns503_OnHttpRequestException()
    {
        // Arrange
        _hackerNewsServiceMock.Setup(service => service.GetNewStoriesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException());

        // Act
        var result = await _controller.GetNewStories(1, 1) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(503, result.StatusCode);
        Assert.Equal("Error occurred while retrieving data from Hacker API.", result.Value);
    }

    [Fact]
    public async Task GetNewStories_Returns500_OnException()
    {
        // Arrange
        _hackerNewsServiceMock.Setup(service => service.GetNewStoriesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception());

        // Act
        var result = await _controller.GetNewStories(1, 1) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("An unexpected error occurred.", result.Value);
    }
}
