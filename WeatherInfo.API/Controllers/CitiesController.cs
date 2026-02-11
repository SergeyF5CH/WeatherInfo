using Microsoft.AspNetCore.Mvc;
using WeatherInfo.API.Services;

namespace WeatherInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly IGeocodingService _geocodingService;

        public CitiesController(IGeocodingService geocodingService)
        {
            _geocodingService = geocodingService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("Query parameter is 'city' is required.");
            }

            if(city.Length < 3)
            {
                return BadRequest("Query parameter 'city' must be at least 3 characters long.");
            }

            var result = await _geocodingService.GetSearchingsAsync(city);

            return Ok(result.Take(10));
        }
    }
}
