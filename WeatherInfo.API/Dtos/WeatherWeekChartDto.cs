namespace WeatherInfo.API.Dtos
{
    public class WeatherWeekChartDto
    {
        public string City { get; set; } = string.Empty;
        public List<ChartDayDto> Days { get; set; } = new List<ChartDayDto>();
    }
}
