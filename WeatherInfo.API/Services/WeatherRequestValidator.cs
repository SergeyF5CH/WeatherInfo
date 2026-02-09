using WeatherInfo.API.Exceptions;

namespace WeatherInfo.API.Services
{
    public class WeatherRequestValidator : IWeatherRequestValidator
    {
        public void ValidateCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                throw new InvalidDateFormatException("City name cannot be empty");
            }
        }
    }
}
