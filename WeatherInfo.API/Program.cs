using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using WeatherInfo.API.DbContexts;
using WeatherInfo.API.HealthChecks;
using WeatherInfo.API.Middleware;
using WeatherInfo.API.ModelBinders;
using WeatherInfo.API.Options;
using WeatherInfo.API.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/weatherinfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Service is running"))
    .AddDbContextCheck<WeatherInfoContext>(
        name: "sqlite-db",
        failureStatus: HealthStatus.Unhealthy)
    .AddCheck<MemoryCacheHealthCheck>(
        name: "memory-cache",
        failureStatus: HealthStatus.Degraded);

// Add services to the container.

builder.Services.AddControllers(options =>
    options.ModelBinderProviders.Insert(0, new DateOnlyModelBinderProvider())
);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<WeatherInfoContext>(
    dbContextOptions => dbContextOptions.UseSqlite(
        builder.Configuration["ConnectionStrings:WeatherInfoDBConnectionString"]));

builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("WeatherClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    var jitterer = new Random();
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 2,
            sleepDurationProvider: attempt =>
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                var jitter = TimeSpan.FromMilliseconds(jitterer.Next(0, 200));
                return delay + jitter;
            },
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                Log.Warning("Http retry {RetryAttempt} after {Delay} sec due to {Reason}",
                    retryAttempt,
                    timespan.TotalSeconds,
                    outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (outcome, delay) =>
        {
            Log.Error(
                "Circuit breaker OPEN for {BreakTime}s. Reason: {Reason}",
                delay.TotalSeconds,
                outcome.Exception?.Message ??
                outcome.Result.StatusCode.ToString());
        },
        onReset: () =>
        {
            Log.Information("Circuit breaker CLOSED (recovered)");
        },
        onHalfOpen: () =>
        {
            Log.Warning("Circuit breaker HALF-OPEN: testing upstream");
        });
}

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IOpenMeteoService, OpenMeteoService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IGeocodingService, GeocodingService>();
builder.Services.AddScoped<IWeatherCodeMapper, WeatherCodeMapper>();
builder.Services.AddScoped<IWeatherRequestValidator, WeatherRequestValidator>();
builder.Services.AddScoped<ICityNormalizer, CityNormalizer>();
builder.Services.AddScoped<IWeatherRequestLogger, WeatherRequestLogger>();

builder.Services.Configure<OpenMeteoOptions>(
    builder.Configuration.GetSection("OpenMeteo"));
builder.Services.Configure<GeocodingOptions>(
    builder.Configuration.GetSection("Geocoding"));
builder.Services.Configure<CacheOptions>(
    builder.Configuration.GetSection("CacheOptions"));

builder.Services.AddMemoryCache();

var app = builder.Build();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".png"] = "image/png";

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/static/icons",
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","icons")),
    ContentTypeProvider = provider
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Name == "self"
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Name != "self"
});

app.Run();
