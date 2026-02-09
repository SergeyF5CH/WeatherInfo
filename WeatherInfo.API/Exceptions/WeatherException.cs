namespace WeatherInfo.API.Exсeptions
{
    public class WeatherException : Exception
    {
        protected WeatherException(string message)
            : base(message)
        {
        }

        protected WeatherException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
