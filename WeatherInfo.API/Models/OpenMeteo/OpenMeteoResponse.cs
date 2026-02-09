using System.Collections.Generic;

namespace WeatherInfo.API.Models.OpenMeteo
{
    public class OpenMeteoResponse
    {
        public Daily Daily { get; set; } = new();
    }
}
