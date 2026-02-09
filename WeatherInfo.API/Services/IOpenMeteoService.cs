using WeatherInfo.API.Models.OpenMeteo;

namespace WeatherInfo.API.Services
{
    public interface IOpenMeteoService
    {
        Task<OpenMeteoResponse> GetByCoordsAsync(
            double latitude,
            double longitude,
            DateOnly date);
        Task<OpenMeteoResponse> Get7DaysByCoordsAsync(
            double latitude,
            double longitude);
    }
}
