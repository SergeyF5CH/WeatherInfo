using Microsoft.AspNetCore.Mvc;
using WeatherInfo.API.Extensions;
using WeatherInfo.API.Infrastructure;
using WeatherInfo.API.Services;

namespace WeatherInfo.API.Controllers
{
    [ApiController]
    [Route("api/weather")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;

        public WeatherController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [HttpGet("{city}")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetWeatherByDate(
            string city,
            [FromQuery] DateOnly? date)
        {
            if (date == null)
            {
                return BadRequest("Date query parameter is required");
            }

            var result = await _weatherService.GetWeatherByDayAsync(city, date.Value);

            var etagSourсe = $"{result.City}:{result.Date}:{result.TemperatureC}:{result.Condition}";
            var etag = $"\"{ETagHelper.Generate(etagSourсe)}\"";

            if (Request.Headers.IfNoneMatch.Any(x => x == etag))
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            Response.Headers.ETag = etag;
            
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            result.AddAbsoluteIconUrl(baseUrl);

            return Ok(result);
        }

        [HttpGet("{city}/week")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetWeatherByWeek(string city)
        {
            var result = await _weatherService.GetWeatherWeekAsync(city);

            var etagSourсe = $"{result.City}:{string.Join('|', result.Days.Select(
                d => $"{d.Date}:{d.TemperatureC}:{d.Condition}"
                ))}";
            var etag = $"\"{ETagHelper.Generate(etagSourсe)}\"";

            if (Request.Headers.IfNoneMatch.Any(x => x == etag)) 
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            Response.Headers.ETag = etag;

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            result.AddAbsoluteIconUrl(baseUrl);

            return Ok(result);
        }

        [HttpGet("{city}/week/chart")]
        public async Task<IActionResult> GetWeatherWeekChart(string city)
        {
            var result = await _weatherService.GetWeatherWeekChartAsync(city);
            return Ok(result);
        }
    }
}
