namespace WeatherInfo.API.Services
{
    public interface IGeocodingService
    {
        Task<(double lat, double lon)> GetCoordinatesAsync(string city);
    }
}
