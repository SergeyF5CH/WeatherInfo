using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using WeatherInfo.API.DbContexts;
using WeatherInfo.API.Dtos;

namespace WeatherInfo.API.Controllers
{
    [ApiController]
    [Route("api/stats")]
    [EnableRateLimiting("PerIp60PerMinute")]
    public class StatsController : ControllerBase
    {
        private readonly WeatherInfoContext _context;

        public StatsController(WeatherInfoContext context)
        {
            _context = context;
        }

        [HttpGet("top-cities")]
        public async Task<IActionResult> GetTopCities(
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            [FromQuery] int limit = 10)
        {
            if(from > to)
            {
                return BadRequest("'from' must be earlier than 'to'");
            }

            var fromUtc = from.ToDateTime(TimeOnly.MinValue);
            var toUtc = to.ToDateTime(TimeOnly.MaxValue);

            var result = await _context.WeatherRequests
                .Where(x => x.TimestampUtc >= fromUtc && x.TimestampUtc <= toUtc)
                .GroupBy(x => x.City)
                .Select(g => new StatsRequestDto 
                {
                    City = g.Key,
                    RequestsCount = g.Count(),
                })
                .OrderByDescending(x => x.RequestsCount)
                .Take(limit)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetResult(
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (from > to)
            {
                return BadRequest("'From' must be earlier than 'to'");
            }
            if(page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and pageSize must be positive");
            }

            var fromUtc = from.ToDateTime(TimeOnly.MinValue);
            var toUtc = to.ToDateTime(TimeOnly.MaxValue);

            var query = _context.WeatherRequests
                .Where(x => x.TimestampUtc >= fromUtc && x.TimestampUtc <= toUtc)
                .OrderByDescending(x => x.TimestampUtc);

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1 ) * pageSize)
                .Take(pageSize)
                .Select(x => new WeatherRequestDto
                {
                    TimestampUtc = x.TimestampUtc,
                    Endpoint = x.Endpoint,
                    City = x.City,
                    Date = x.Date,
                    CacheHit = x.CacheHit,
                    StatusCode = x.StatusCode,
                    LatencyMs = x.LatencyMs
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                items
            });
        }
    }
}
