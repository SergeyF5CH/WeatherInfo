using Microsoft.Extensions.Http;
using Moq;
using Moq.Protected;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using System.Net;
using Xunit;

namespace WeatherInfo.API.Tests.Policies
{
    public class CircuitBreakerPolicyTests
    {
        [Fact]
        public async Task CircuitBreaker_ShouldStayClosed_UntilConsecutiveFailuresReached()
        {
            int callCount = 0;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(() =>
                {
                    callCount++;
                    throw new HttpRequestException("Simulated server error");
                });

            var breakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 2,
                    durationOfBreak: TimeSpan.FromSeconds(5)
                );

            var policyHandler = new PolicyHttpMessageHandler(breakerPolicy)
            {
                InnerHandler = handlerMock.Object
            };

            var httpClient = new HttpClient(policyHandler);

            for(int i = 0; i <= 1; i++)
            {
                await Assert.ThrowsAsync<HttpRequestException>(() => httpClient.GetAsync("https://fake.url"));
            }
            await Assert.ThrowsAsync<BrokenCircuitException>(() => httpClient.GetAsync("https://fake.url"));
            
            Assert.Equal(2, callCount);
        }
    }
}
