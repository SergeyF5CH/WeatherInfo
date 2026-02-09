using WeatherInfo.API.Exсeptions;

namespace WeatherInfo.API.Exceptions
{
    public class UpstreamUnavailableException : WeatherException
    {
        public string Provider { get; }

        public UpstreamUnavailableException(string provider)
            : base($"Upstream service '{provider}' is unavailable")
        {
            Provider = provider;
        }

        public UpstreamUnavailableException(string provider, Exception innerException)
            : base($"Upstream service '{provider}' is unavailable", innerException)
        {
            Provider = provider;
        }
    }
}
