using WeatherInfo.API.Exсeptions;

namespace WeatherInfo.API.Exceptions
{
    public class UpstreamBadRequestException : WeatherException
    {
        public string ServiceName { get; }

        public UpstreamBadRequestException(string serviceName, string? details = null)
            : base($"Upstream service '{serviceName}' rejected request")
        {
            ServiceName = serviceName;
            if(details != null)
            {
                Data["Details"] = details;
            }
        }
    }
}
