namespace WeatherInfo.API.Dtos
{
    public class WeatherWeekDayDto
    {
        public DateOnly Date { get; set; }
        public string Condition { get; set; } = string.Empty;
        public double TemperatureC { get; set; }
        public string IconUrl { get; set; } = string.Empty;
    }
}
