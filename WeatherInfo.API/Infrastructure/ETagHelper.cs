using System.Security.Cryptography;
using System.Text;

namespace WeatherInfo.API.Infrastructure
{
    public class ETagHelper
    {
        public static string Generate(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
