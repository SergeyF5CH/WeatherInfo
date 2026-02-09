namespace WeatherInfo.API.Caching
{
    public class CacheEntry<T>
    {
        public T Value { get; init; } = default!;
        public DateTime ExpireAtUtc { get; init; }
        public bool IsRefreshing { get; set; }
    }
}
