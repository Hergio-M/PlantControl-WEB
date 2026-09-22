using System.ComponentModel.DataAnnotations;

namespace PlantControlWeb.Models
{
    public class TelemetryRequest
    {
        [Range(-40, 80)]
        public decimal? TemperatureAir { get; set; }

        [Range(-40, 80)]
        public decimal? TemperatureSoil { get; set; }

        [Range(0, 100)]
        public decimal? Humidity { get; set; }

        [Range(0, 100)]
        public decimal? SoilMoisture { get; set; }

        [Range(0, 100000)]
        public decimal? Light { get; set; }
    }
}
