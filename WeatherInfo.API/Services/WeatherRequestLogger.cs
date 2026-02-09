using WeatherInfo.API.DbContexts;
using WeatherInfo.API.Entities;

namespace WeatherInfo.API.Services
{
    public class WeatherRequestLogger : IWeatherRequestLogger
    {
        private readonly WeatherInfoContext _context;

        public WeatherRequestLogger(WeatherInfoContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task LogAsync(WeatherRequestLog log)
        {
            _context.WeatherRequests.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
