using Microsoft.Extensions.Options;
using Serilog;
using System.Globalization;
using System.Net;
using WeatherInfo.API.Exceptions;
using WeatherInfo.API.Models.OpenMeteo;
using WeatherInfo.API.Options;

namespace WeatherInfo.API.Services
{
    public class OpenMeteoService : IOpenMeteoService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenMeteoOptions _settings;

        public OpenMeteoService(
            IHttpClientFactory httpClientFactory,
            IOptions<OpenMeteoOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
        }

        public async Task<OpenMeteoResponse> GetByCoordsAsync(
            double latitude,
            double longitude,
            DateOnly date)
        { 
            var dateStr = date.ToString("yyyy-MM-dd");

            var url = $"{_settings.BaseUrl}?" +
                  $"latitude={latitude.ToString(CultureInfo.InvariantCulture)}&" +
                  $"longitude={longitude.ToString(CultureInfo.InvariantCulture)}&" +
                  $"daily=temperature_2m_max,weathercode&" +
                  $"timezone={_settings.Timezone}&start_date={dateStr}&end_date={dateStr}";

            return await FetchWeatherAsync(url);
        }
            

        public async Task<OpenMeteoResponse> Get7DaysByCoordsAsync(
            double latitude,
            double longitude)
        {
            var url = $"{_settings.BaseUrl}?" +
                  $"latitude={latitude.ToString(CultureInfo.InvariantCulture)}&" +
                  $"longitude={longitude.ToString(CultureInfo.InvariantCulture)}&" +
                  $"daily=temperature_2m_max,weathercode&timezone={_settings.Timezone}";

            return await FetchWeatherAsync(url);
        }

        private async Task<OpenMeteoResponse> FetchWeatherAsync(string url)
        {
            var client = _httpClientFactory.CreateClient("WeatherClient");

            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync(url);
            }
            catch (Exception ex) 
            {
                Log.Error(ex, "Failed to reach Open-Meteo");
                throw new UpstreamUnavailableException("Open-Meteo", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();

                Log.Warning(
                    "Open-Meteo returned {StatusCode} for {Url}. Body: {Body}",
                    (int)response.StatusCode,
                    url,
                    body);

                if((int)response.StatusCode < 500)
                {
                    throw new UpstreamBadRequestException("Open-Meteo", body);
                }

                throw new UpstreamUnavailableException("Open-Meteo");
            }

            var result = await response.Content.ReadFromJsonAsync<OpenMeteoResponse>();
            
            if(result?.Daily?.Temperature_2m_max == null || result.Daily.Temperature_2m_max.Count == 0)
            {
                Log.Warning("Open-Meteo returned empty data for URL {Url}", url);
                throw new UpstreamUnavailableException("Open-Meteo");
            }

            return result;
        }
    }
}
