using HackerWebAPI.Interface;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class HackerNewsService : IHackerNewsService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cacheStory;
    private const string CacheKey = "newstories";
    private const string BaseURL = "https://hacker-news.firebaseio.com/v0/";

    public HackerNewsService(IHttpClientFactory httpClientFactory, IMemoryCache cacheStory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _cacheStory = cacheStory;
    }

    public async Task<(int total, IEnumerable<object> newStories)> GetNewStoriesAsync(int page, int pageSize)
    {
        List<int> newStoryIds;
        if (!_cacheStory.TryGetValue(CacheKey, out newStoryIds))
        {
            // Get new story ids
            var result = await _httpClient.GetStringAsync(BaseURL + "topstories.json?print=pretty");
            newStoryIds = JsonConvert.DeserializeObject<List<int>>(result);
            var cacheStoryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));
            _cacheStory.Set(CacheKey, newStoryIds, cacheStoryOptions);
        }

        var pageIds = newStoryIds.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        // Fetch story details
        var storyResult = pageIds.Select(async id =>
        {
            var storyResponse = await _httpClient.GetStringAsync(BaseURL + $"item/{id}.json?print=pretty");
            return JsonConvert.DeserializeObject<object>(storyResponse);
        });

        var newStories = await Task.WhenAll(storyResult);
        return (pageIds.Count, newStories);
    }
}
