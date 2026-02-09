using Moq;
using Microsoft.Extensions.Caching.Memory;
using WeatherInfo.API.Dtos;
using WeatherInfo.API.Services;
using WeatherInfo.API.Options;
using Xunit;
using WeatherInfo.API.Caching;

namespace WeatherInfo.API.Tests.Services
{
    public class WeatherServiceCacheTests
    {
        [Fact]
        public async Task GetWeatherByDayAsync_WhenWeatherIsCached_ReturnsDateFromCache()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var city = "Vladivostok";
            var normalizedCity = city.ToLowerInvariant();
            var date = DateOnly.FromDateTime(DateTime.UtcNow);

            var cachedWeather = new WeatherDayDto
            {
                City = normalizedCity,
                Date = date,
                TemperatureC = 20,
                IconUrl = "clear",
                FetchedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var cacheKey = $"weather-day:open-meteo:{normalizedCity}:{date:yyyy-MM-dd}";

            memoryCache.Set(
                cacheKey, 
                new CacheEntry<WeatherDayDto>
                {
                    Value = cachedWeather,
                    ExpireAtUtc = DateTime.UtcNow.AddMinutes(60),
                    IsRefreshing = false
                },
                TimeSpan.FromMinutes(60));

            var cityNormalizerMock = new Mock<ICityNormalizer>();
            cityNormalizerMock
                .Setup(x => x.Normalize(city))
                .Returns(normalizedCity);

            var service = new WeatherService(
                memoryCache,
                Mock.Of<IGeocodingService>(),
                Mock.Of<IOpenMeteoService>(),
                Mock.Of<IWeatherCodeMapper>(),
                Mock.Of<IWeatherRequestValidator>(),
                Microsoft.Extensions.Options.Options.Create(
                    new CacheOptions 
                    { 
                        WeatherTtlMinutes = 60,
                        RefreshAheadMinutes = 5
                    }),
                cityNormalizerMock.Object,
                Mock.Of<IWeatherRequestLogger>()
                );

            var result = await service.GetWeatherByDayAsync(city, date);

            Assert.Equal(cachedWeather, result);
        }
    }
}
