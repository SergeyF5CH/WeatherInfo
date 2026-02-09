using WeatherInfo.API.Services;
using Xunit;

namespace WeatherInfo.API.Tests.Services
{
    public class WeatherCodeMapperTests
    {
        [Theory]
        [InlineData(0, "clear")]
        [InlineData(2, "cloudy")]
        [InlineData(51, "drizzle")]
        [InlineData(63, "rain")]
        [InlineData(71, "snow")]
        [InlineData(80, "rain_snowers")]
        [InlineData(999, "cloudy")]
        public void Map_ValidData_ReturnsCorrectIconCode(int code, string expected)
        {
            var mapper = new WeatherCodeMapper();

            var result = mapper.Map(code);

            Assert.Equal(expected, result);
        }
    }
}
