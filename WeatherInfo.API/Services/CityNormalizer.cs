using System.Globalization;
using System.Text.RegularExpressions;

namespace WeatherInfo.API.Services
{
    public class CityNormalizer : ICityNormalizer
    {
        private static readonly TextInfo InvariantTextInfo =
            CultureInfo.InvariantCulture.TextInfo;

        public string Normalize(string city) 
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return string.Empty;
            }

            var normalized = city.Trim().ToLowerInvariant();
            normalized = Regex.Replace(normalized, @"\s+", " ");
            normalized = InvariantTextInfo.ToTitleCase(normalized);

            return normalized;
        }
    }
}
