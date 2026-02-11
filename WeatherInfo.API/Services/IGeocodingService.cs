using WeatherInfo.API.Dtos;

namespace WeatherInfo.API.Services
{
    public interface IGeocodingService
    {
        Task<(double lat, double lon)> GetCoordinatesAsync(string city);
        Task<IEnumerable<SearchingCitiesDto>> GetSearchingsAsync(string query);
    }
}
