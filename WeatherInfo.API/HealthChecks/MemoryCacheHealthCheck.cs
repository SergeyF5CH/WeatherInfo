using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WeatherInfo.API.HealthChecks
{
    public class MemoryCacheHealthCheck : IHealthCheck
    {
        private readonly IMemoryCache _cache;
        public MemoryCacheHealthCheck(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _cache != null
                    ? HealthCheckResult.Healthy("MemoryCache is available")
                    : HealthCheckResult.Unhealthy("MemoryCache not available"));
        }
    }
}
