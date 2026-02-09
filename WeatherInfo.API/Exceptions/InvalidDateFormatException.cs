using WeatherInfo.API.Exсeptions;

namespace WeatherInfo.API.Exceptions
{
    public class InvalidDateFormatException : WeatherException
    {
        public string Date { get; }
        public InvalidDateFormatException(string date) 
            : base($"Invalid date format: '{date}'")
        {
            Data["Details"] = $"Expected format: yyyy-MM-dd";
        }
    }
}
