using WeatherInfo.API.Entities;

namespace WeatherInfo.API.Services
{
    public interface IWeatherRequestLogger
    {
        Task LogAsync(WeatherRequestLog log);
    }
}
