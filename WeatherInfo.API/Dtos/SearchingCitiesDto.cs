namespace WeatherInfo.API.Dtos
{
    public record SearchingCitiesDto(
        string City,
        double Latitude,
        double Longitude
    );
}
