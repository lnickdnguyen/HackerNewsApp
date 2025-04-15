namespace HackerNewsApp.Controllers
{
    using HackerNewsApp.Services;
    using Microsoft.AspNetCore.Mvc;

    namespace HackerNewsApi.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class HackerNewsController : ControllerBase
        {
            private readonly IHackerNewsService _service;

            public HackerNewsController(IHackerNewsService service)
            {
                _service = service;
            }

            [HttpGet("getNewestStories")]
            public async Task<IActionResult> GetNewestStories(
                [FromQuery] int page = 1, 
                [FromQuery] int pageSize = 20, 
                [FromQuery] string search = null)
            {
                var stories = await _service.GetNewestStoriesAsync(page, pageSize, search);
                return Ok(stories);
            }
        }
    }
}
