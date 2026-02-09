namespace WeatherInfo.API.Services
{
    public interface IWeatherRequestValidator
    {
        void ValidateCity(string city);
    }
}
