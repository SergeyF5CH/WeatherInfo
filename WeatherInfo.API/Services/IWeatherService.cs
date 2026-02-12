using WeatherInfo.API.Dtos;

namespace WeatherInfo.API.Services
{
    public interface IWeatherService
    {
        Task<WeatherDayDto> GetWeatherByDayAsync(string city, DateOnly date);
        Task<WeatherWeekDto> GetWeatherWeekAsync(string city, bool logRequest = true);
        Task<WeatherWeekChartDto> GetWeatherWeekChartAsync (string city);
    }
}
