namespace WeatherInfo.API.Dtos
{
    public class StatsRequestDto
    {
        public string City { get; set; } = string.Empty;
        public int RequestsCount { get; set; }
    }
}
