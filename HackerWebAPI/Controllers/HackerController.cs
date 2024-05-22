using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;
using System.Data.SqlTypes;
using System.Net;
using System.Threading.Tasks;

namespace HackerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HackerController : ControllerBase
    {
        private readonly HttpClient _httpclient;
        private readonly IMemoryCache _cacheStory;
        private const string CacheKey = "newstories";
        private const string BaseURL = "https://hacker-news.firebaseio.com/v0/";
        public HackerController(IHttpClientFactory httpClientFactory, IMemoryCache cacheStory)
        {
            _httpclient = httpClientFactory.CreateClient();
            _cacheStory = cacheStory;
        }
        /// <summary>
        /// GetNewStories
        /// </summary>
        /// <param name="page">1</param>
        /// <param name="pageSize">10</param>
        /// <returns>Return all new stories and their count</returns>
        [HttpGet("newstories")]
        [SwaggerResponse(500, "An unexpected error occurred.")]
        [SwaggerResponse(200, "Ok")]
        [SwaggerResponse(503, "Error occurred while retrieving data from Hacker API.")]
        public async Task<IActionResult> GetNewStories([FromQuery] int page = 1, [FromQuery] int pageSize = 200)
        {
            try
            {
                List<int> newStoryIds;
                if (!_cacheStory.TryGetValue(CacheKey, out newStoryIds))
                {
                    //Get new story ids
                    var result = await _httpclient.GetStringAsync(BaseURL + "topstories.json?print=pretty");
                    newStoryIds = JsonConvert.DeserializeObject<List<int>>(result);
                    var cacheStoryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5));
                    _cacheStory.Set(CacheKey, newStoryIds, cacheStoryOptions);
                }
                var pageIds = newStoryIds.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                // new story ids pass to get story to fetch the details of the story 
                var storyResult = pageIds.Select(async x =>
                 {
                     var getstory = await _httpclient.GetStringAsync(BaseURL + $"item/{x}.json?print=pretty");
                     return JsonConvert.DeserializeObject<object>(getstory);
                 });
                var newstories = await Task.WhenAll(storyResult);

                return Ok(new { Total = pageIds.Count, NewStories = newstories });
            }
            catch (HttpRequestException httpEx)
            {
                return StatusCode(503, "Error occurred while retrieving data from Hacker API.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }


    }
}
