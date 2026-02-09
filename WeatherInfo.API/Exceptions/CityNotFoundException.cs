using WeatherInfo.API.Exсeptions;

namespace WeatherInfo.API.Exceptions
{
    public class CityNotFoundException : WeatherException
    {
        public string City { get; }
        public CityNotFoundException(string city) 
            : base($"City '{city}' not found")
        {
            City = city;
            Data["Details"] = $"City '{city}' not found in provider";
        }
    }
}
