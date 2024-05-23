using HackerWebAPI.Interface;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HackerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HackerController : ControllerBase
    {
        private readonly IHackerNewsService _hackerNewsService;

        public HackerController(IHackerNewsService hackerNewsService)
        {
            _hackerNewsService = hackerNewsService;
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
                var (total, newStories) = await _hackerNewsService.GetNewStoriesAsync(page, pageSize);
                return Ok(new NewStoriesResponse { Total = total, NewStories = newStories });
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "Error occurred while retrieving data from Hacker API.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

    }
}
