using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WeatherInfo.API.Controllers;
using WeatherInfo.API.Dtos;
using WeatherInfo.API.Services;
using Xunit;

namespace WeatherInfo.API.Tests.Controllers
{
    public class WeatherControllerEtagTests
    {
        [Fact]
        public async Task GetWeatherByDate_WhenEtagMatches_Returns304()
        {
            var city = "Саранск";
            var date = DateOnly.FromDateTime(DateTime.UtcNow);

            var weather = new WeatherDayDto
            {
                City = city,
                Date = date,
                TemperatureC = 32,
                Condition = "clear"
            };

            var weatherServiceMock = new Mock<IWeatherService>();
            weatherServiceMock
                .Setup(x => x.GetWeatherByDayAsync(city, date))
                .ReturnsAsync(weather);

            var controller = new WeatherController(weatherServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            

            var etagSource = $"{weather.City}:{weather.Date}:{weather.TemperatureC}:{weather.Condition}";
            var etag = $"\"{Infrastructure.ETagHelper.Generate(etagSource)}\"";

            controller.Request.Headers.IfNoneMatch = etag;

            var result = await controller.GetWeatherByDate(city, date);

            var statusResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status304NotModified, statusResult.StatusCode);
        }
    }
}
