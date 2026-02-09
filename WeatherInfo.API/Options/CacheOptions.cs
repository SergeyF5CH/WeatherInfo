namespace WeatherInfo.API.Options
{
    public class CacheOptions
    {
        public int WeatherTtlMinutes { get; set; }
        public int RefreshAheadMinutes { get; set; } 
    }
}
