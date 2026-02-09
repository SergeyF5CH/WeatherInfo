using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeatherInfo.API.Models.OpenMeteo
{
    public class Daily
    {
        [JsonPropertyName("temperature_2m_max")]
        public List<double> Temperature_2m_max { get; set; } = new List<double>();

        [JsonPropertyName("weathercode")]
        public List<int> WeatherCode { get; set; } = new List<int>();
    }
}
