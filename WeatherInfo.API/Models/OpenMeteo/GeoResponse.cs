namespace WeatherInfo.API.Models.OpenMeteo
{
    public class GeoResponse
    {
        public List<GeoResult> Results { get; set; } = new List<GeoResult>();
    }

    public class GeoResult
    {
        public string City { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Country { get; set; } = string.Empty;
    }
}
