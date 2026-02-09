using Microsoft.Extensions.Http;
using Moq;
using Moq.Protected;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using Xunit;

namespace WeatherInfo.API.Tests.Policies
{
    public class RetryPolicyExtensionsTests
    {
        [Fact]
        public async Task FetchWeather_TransientError_SuccessAfterRetry() 
        {
            int callCount = 0;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                    )
                .Returns(async () =>
                {
                    callCount++;
                    if (callCount <= 1)
                    {
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{ \"daily\": { \"temperature_2m_max\": [20], \"weathercode\": [0] } }")
                    };
                });

            var client = new HttpClient(handlerMock.Object);
            client.Timeout = TimeSpan.FromSeconds(1);

            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(2, attempt => TimeSpan.Zero);

            var policyClient = new PolicyHttpMessageHandler(policy)
            {
                InnerHandler = handlerMock.Object,
            };

            var httpClient = new HttpClient(policyClient);
            var response = await httpClient.GetAsync("https://fake.url");

            Assert.Equal(2, callCount);
        }
    }
}
