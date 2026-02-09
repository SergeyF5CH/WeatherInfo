namespace WeatherInfo.API.Dtos
{
    public class WeatherWeekDto
    {
        public string City { get; set; } = string.Empty;
        public List<WeatherWeekDayDto> Days { get; set; } = new List<WeatherWeekDayDto>();
        public string Source { get; set; } = string.Empty;
        public DateTime FetchedAt { get; set; }
    }
}
