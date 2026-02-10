namespace WeatherInfo.API.Services
{
    public class WeatherCodeMapper : IWeatherCodeMapper
    {
        public string Map(int code)
        {
            return code switch
            {
                0 or 1 => "clear",
                2 or 3 => "cloudy",
                45 or 48 => "fog",
                51 or 53 or 55 => "drizzle",
                61 or 63 or 65 => "rain",
                71 or 73 or 75 => "snow",
                80 or 81 or 82 => "rain_snowers",
                95 or 96 or 99 => "thunderstorm",
                _ => "cloudy"
            };
        }
    }
}

    //0 => "clear_sky", 
    //1 => "mainly_clear", 
    //2 => "partly_cloudy", 
    //3 => "overcast", 
    //45 => "fog", 
    //48 => "depositing_rime_fog", 
    //51 => "drizzle_light", 
    //53 => "drizzle_moderate", 
    //55 => "drizzle_dense", 
    //61 => "rain_slight", 
    //63 => "rain_moderate", 
    //65 => "rain_heavy",
    //71 => "snow_slight", 
    //73 => "snow_moderate",
    //75 => "snow_heavy", 
    //80 => "rain_showers_slight", 
    //81 => "rain_showers_moderate", 
    //82 => "rain_showers_violent",
