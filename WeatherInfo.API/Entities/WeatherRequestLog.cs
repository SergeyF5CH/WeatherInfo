using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherInfo.API.Entities
{
    public class WeatherRequestLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime TimestampUtc { get; set; }
        
        [Required]
        public string Endpoint { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string City { get; set; }
        public DateOnly? Date { get; set; }
        public bool CacheHit { get; set; }
        public int StatusCode { get; set; }
        public int LatencyMs { get; set; }
    }
}
