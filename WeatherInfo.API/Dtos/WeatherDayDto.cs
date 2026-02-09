namespace WeatherInfo.API.Dtos
{
    public class WeatherDayDto
    {
        public string City { get; set; } = string.Empty;
        public DateOnly Date {  get; set; }
        public string Condition { get; set; } = string.Empty;
        public double TemperatureC { get; set; }
        public string IconUrl { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime FetchedAt { get; set; }
    }
}
