using WeatherInfo.API.Controllers;
using WeatherInfo.API.Entities;
using WeatherInfo.API.Models;
using Microsoft.EntityFrameworkCore;


namespace WeatherInfo.API.DbContexts
{
    public class WeatherInfoContext : DbContext
    {
        public WeatherInfoContext(DbContextOptions<WeatherInfoContext> options)
            : base(options) 
        { 
        }

        public DbSet<WeatherRequestLog> WeatherRequests => Set<WeatherRequestLog>();
    }
}
