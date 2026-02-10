using WeatherInfo.API.Dtos;

namespace WeatherInfo.API.Extensions
{
    public static class WeatherDtoExtensions
    {
        public static void AddAbsoluteIconUrl(this WeatherDayDto dto, string baseUrl)
        {
            if (string.IsNullOrEmpty(dto.IconUrl))
            {
                return;
            }

            if (dto.IconUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            dto.IconUrl = $"{baseUrl}{dto.IconUrl}";
        }

        public static void AddAbsoluteIconUrl(this WeatherWeekDayDto dto, string baseUrl)
        {
            if (string.IsNullOrEmpty(dto.IconUrl))
            {
                return;
            }

            if (dto.IconUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            dto.IconUrl = $"{baseUrl}{dto.IconUrl}";
        }

        public static void AddAbsoluteIconUrl(this WeatherWeekDto dto, string baseUrl)
        {
            foreach (var day in dto.Days)
            {
                day.AddAbsoluteIconUrl(baseUrl);
            }
        }
    }
}
