using Microsoft.AspNetCore.Mvc;
using WeatherInfo.API.Extensions;
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
        public async Task<IActionResult> GetWeatherByDate(
            string city,
            [FromQuery] DateOnly? date)
        {
            if (date == null)
            {
                return BadRequest("Date query parameter is required");
            }

            var result = await _weatherService.GetWeatherByDayAsync(city, date.Value);
            
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            result.AddAbsoluteIconUrl(baseUrl);

            return Ok(result);
        }

        [HttpGet("{city}/week")]
        public async Task<IActionResult> GetWeatherByWeek(string city)
        {
            var result = await _weatherService.GetWeatherWeekAsync(city);

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
