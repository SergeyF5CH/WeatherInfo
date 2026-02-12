using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Serilog;
using System.Diagnostics;
using WeatherInfo.API.Caching;
using WeatherInfo.API.Dtos;
using WeatherInfo.API.Entities;
using WeatherInfo.API.Options;

namespace WeatherInfo.API.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IMemoryCache _cache;
        private readonly IGeocodingService _geocodingService;
        private readonly IOpenMeteoService _openMeteoService;
        private readonly IWeatherCodeMapper _weatherCodeMapper;
        private readonly IWeatherRequestValidator _weatherRequestValidator;
        private readonly IOptions<CacheOptions> _cacheOptions;
        private readonly ICityNormalizer _cityNormalizer;
        private readonly IWeatherRequestLogger _requestLogger;

        public WeatherService(
            IMemoryCache cache,
            IGeocodingService geocodingService, 
            IOpenMeteoService openMeteoService,
            IWeatherCodeMapper weatherCodeMapper,
            IWeatherRequestValidator weatherRequestValidator,
            IOptions<CacheOptions> cacheOptions,
            ICityNormalizer cityNormalizer,
            IWeatherRequestLogger requestLogger)
        {
            _cache = cache;
            _geocodingService = geocodingService;
            _openMeteoService = openMeteoService;
            _weatherCodeMapper = weatherCodeMapper;
            _weatherRequestValidator = weatherRequestValidator;
            _cacheOptions = cacheOptions;
            _cityNormalizer = cityNormalizer;
            _requestLogger = requestLogger;
        }

        public async Task<WeatherDayDto> GetWeatherByDayAsync(string city, DateOnly date)
        {
            var sw = Stopwatch.StartNew();

            _weatherRequestValidator.ValidateCity(city);

            var cityNormalized = _cityNormalizer.Normalize(city);
            var cacheKey = BuildCachedKey("day", cityNormalized, date.ToString("yyyy-MM-dd"));

            var (result, cacheHit) = await GetOrRefreshCacheAsync(
                cacheKey,
                async () =>
                {
                    var (lat, lon) = await _geocodingService.GetCoordinatesAsync(city);
                    var response = await _openMeteoService.GetByCoordsAsync(lat, lon, date);

                    return new WeatherDayDto
                    {
                        City = cityNormalized,
                        Date = date,
                        Condition = _weatherCodeMapper.Map(response.Daily.WeatherCode[0]),
                        TemperatureC = response.Daily.Temperature_2m_max[0],
                        IconUrl = $"/static/icons/{_weatherCodeMapper.Map(response.Daily.WeatherCode[0])}.png",
                        Source = "open-meteo",
                        FetchedAt = DateTime.UtcNow
                    };
                });

            sw.Stop();
            await LogRequestAsync(
                "day", 
                cityNormalized, 
                date,
                cacheHit,
                (int)sw.ElapsedMilliseconds,
                200);
       
            return result;
        }

        public async Task<WeatherWeekDto> GetWeatherWeekAsync(string city, bool logRequest = true)
        {
            var sw = Stopwatch.StartNew();

            _weatherRequestValidator.ValidateCity(city);

            var cityNormalized = _cityNormalizer.Normalize(city);
            var cacheKey = BuildCachedKey("week", cityNormalized);

            var (result, cacheHit) = await GetOrRefreshCacheAsync(
                cacheKey,
                async () =>
                {
                    var (latitude, longitude) = await _geocodingService.GetCoordinatesAsync(city);
                    var response = await _openMeteoService.Get7DaysByCoordsAsync(latitude, longitude);

                    var weekDto = new WeatherWeekDto
                    {
                        City = cityNormalized,
                        Source = "open-meteo",
                        FetchedAt = DateTime.UtcNow

                    };

                    for (var i = 0; i < response.Daily.Temperature_2m_max.Count; i++)
                    {
                        weekDto.Days.Add(new WeatherWeekDayDto
                        {
                            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i)),
                            Condition = _weatherCodeMapper.Map(response.Daily.WeatherCode[i]),
                            TemperatureC = response.Daily.Temperature_2m_max[i],
                            IconUrl = $"/static/icons/{_weatherCodeMapper.Map(response.Daily.WeatherCode[i])}.png",
                        });
                    }

                    return weekDto;
                });

            sw.Stop();

            if (logRequest)
            {
                await LogRequestAsync(
                    "week",
                    cityNormalized,
                    null,
                    cacheHit,
                    (int)sw.ElapsedMilliseconds,
                    200);
            }

            return result;
        }

        public async Task<WeatherWeekChartDto> GetWeatherWeekChartAsync(string city)
        {
            var sw = Stopwatch.StartNew();
            
            _weatherRequestValidator.ValidateCity(city);

            var cityNormalized = _cityNormalizer.Normalize(city);
            var cacheKey = BuildCachedKey("week-chart", cityNormalized);

            var (result, cacheHit) = await GetOrRefreshCacheAsync(
                cacheKey,
                async () =>
                {
                    var weekData = await GetWeatherWeekAsync(cityNormalized, logRequest: false);
                    return new WeatherWeekChartDto
                    {
                        City = weekData.City,
                        Days = weekData.Days.Select(d => new ChartDayDto
                        {
                            Date = d.Date,
                            TemperatureC = d.TemperatureC
                        })
                        .ToList(),
                    };
                });
            
            sw.Stop();
            await LogRequestAsync(
                "week-chart", 
                cityNormalized, 
                null, 
                cacheHit,
                (int)sw.ElapsedMilliseconds,
                200);
            
            return result;
        }

        private async Task<(T Value, bool CacheHit)> GetOrRefreshCacheAsync<T>(string cacheKey, Func<Task<T>> fetchFreshData)
        {

            if (_cache.TryGetValue(cacheKey, out CacheEntry<T> entry))
            {
                if (ShouldRefresh(entry))
                {
                    TriggerRefreshInBaskground(cacheKey, fetchFreshData, entry);
                }

                return (entry.Value, true);
            }

            var fresh = await fetchFreshData();
            var newEntry = new CacheEntry<T>
            {
                Value = fresh,
                ExpireAtUtc = DateTime.UtcNow.AddMinutes(_cacheOptions.Value.WeatherTtlMinutes),
                IsRefreshing = false
            };

            _cache.Set(cacheKey, newEntry, TimeSpan.FromMinutes(_cacheOptions.Value.WeatherTtlMinutes));
            return (fresh, false);
        }

        private void TriggerRefreshInBaskground<T>(
            string cacheKey, 
            Func<Task<T>> fetchFreshData, 
            CacheEntry<T> entry)
        {
            entry.IsRefreshing = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    var fresh = await fetchFreshData();
                    var newEntry = new CacheEntry<T>
                    {
                        Value = fresh,
                        ExpireAtUtc =
                            DateTime.UtcNow.AddMinutes(_cacheOptions.Value.WeatherTtlMinutes),
                        IsRefreshing = false
                    };
                    _cache.Set(cacheKey, newEntry, TimeSpan.FromMinutes(_cacheOptions.Value.WeatherTtlMinutes));
                }
                catch
                {
                    entry.IsRefreshing = false;
                }
            });
        }

        private bool ShouldRefresh<T>(CacheEntry<T> entry)
        {
            var refreshFrom =
                entry.ExpireAtUtc
                .AddMinutes(-_cacheOptions.Value.RefreshAheadMinutes);

            return DateTime.UtcNow >= refreshFrom
                && !entry.IsRefreshing;
        }
        private string BuildCachedKey(string type, string city, string date = null)
        {
            date ??= DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
            return $"weather-{type}:open-meteo:{city}:{date}";
        }

        private async Task LogRequestAsync(
            string endpoint, 
            string cityNormalized, 
            DateOnly? date, 
            bool cacheHit, 
            int latencyMs,
            int statusCode)
        {
            var cacheStatus = cacheHit ? "TRUE" : "FALSE";

            Log.Information(
                "Endpoint = {Endpoint}, City = {City}, Date = {Date}, Latency = {Latency}ms, CacheHit = {CacheStatus}, StatusCode = {StatusCode}",
                endpoint, 
                cityNormalized,
                date?.ToString("yyyy-MM-dd") ?? "-",
                cacheStatus,
                latencyMs,
                statusCode);

            await _requestLogger.LogAsync(new WeatherRequestLog
            {
                TimestampUtc = DateTime.UtcNow,
                Endpoint = endpoint,
                City = cityNormalized,
                Date = date,
                CacheHit = cacheHit,
                LatencyMs = latencyMs,
                StatusCode = statusCode
            });
        }
    }
}


