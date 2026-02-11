using Microsoft.Extensions.Options;
using Serilog;
using WeatherInfo.API.Dtos;
using WeatherInfo.API.Exceptions;
using WeatherInfo.API.Models.OpenMeteo;
using WeatherInfo.API.Options;

namespace WeatherInfo.API.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GeocodingOptions _geocodingOptions;

        public GeocodingService(IHttpClientFactory httpClientFactory, IOptions<GeocodingOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _geocodingOptions = options.Value;
        }

        public async Task<(double lat, double lon)> GetCoordinatesAsync(string city)
        {
            var client = _httpClientFactory.CreateClient("WeatherClient");

            var url = $"{_geocodingOptions.BaseUrl}" +
                      $"?name={Uri.EscapeDataString(city)}" +
                      $"&count=1&language={_geocodingOptions.Language}";

            HttpResponseMessage httpResponse;

            try
            {
                httpResponse = await client.GetAsync(url);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Failed to reach Geocoding API for city {City}", city);
                throw new UpstreamUnavailableException("Geocoding", ex);
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync();
                Log.Error("Geocoding API returned {StatusCode} for city {City}: {Error}",
                    httpResponse.StatusCode, city, error);
                throw new UpstreamUnavailableException("Geocoding");
            }

            GeoResponse? response;

            try
            {
                response = await httpResponse.Content.ReadFromJsonAsync<GeoResponse>();
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Failed to parse Geocoding API response for city {City}", city);
                throw new UpstreamUnavailableException("Geocoding", ex);
            }

            if(response?.Results == null || response.Results.Count == 0)
            {
                Log.Warning("City not found in Geocoding API: {City}", city);
                throw new CityNotFoundException(city);
            }

            var result = response.Results[0];
            return (result.Latitude, result.Longitude);
        }

        public async Task<IEnumerable<SearchingCitiesDto>> GetSearchingsAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return Enumerable.Empty<SearchingCitiesDto>();
            }

            var client = _httpClientFactory.CreateClient("WeatherClient");

            var url = $"{_geocodingOptions.BaseUrl}" +
                      $"?name={Uri.EscapeDataString(city)}" +
                      $"&count=10&language={_geocodingOptions.Language}";

            HttpResponseMessage httpResponse;

            try
            {
                httpResponse = await client.GetAsync(url);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to reach Geocoding API for search query {City}", city);
                throw new UpstreamUnavailableException("Geocoding", ex);
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync();
                Log.Error("Geocoding API returned {StatusCode} for city {City}: {Error}",
                    httpResponse.StatusCode, city, error);
                throw new UpstreamUnavailableException("Geocoding");
            }

            GeoResponse? response;

            try
            {
                response = await httpResponse.Content.ReadFromJsonAsync<GeoResponse>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to parse Geocoding API response for city {City}", city);
                throw new UpstreamUnavailableException("Geocoding", ex);
            }

            if(response?.Results == null)
            {
                return Enumerable.Empty<SearchingCitiesDto>();
            }

            return response.Results.Select(r => 
                new SearchingCitiesDto(
                    r.City,
                    r.Latitude,
                    r.Longitude
                ));
        }
    }
}
