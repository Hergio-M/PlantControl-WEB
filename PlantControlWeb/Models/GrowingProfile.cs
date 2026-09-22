namespace PlantControlWeb.Models
{
    public class GrowingProfile
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal? TemperatureAirMin { get; set; }
        public decimal? TemperatureAirMax { get; set; }

        public decimal? TemperatureSoilMin { get; set; }
        public decimal? TemperatureSoilMax { get; set; }

        public decimal? HumidityMin { get; set; }
        public decimal? HumidityMax { get; set; }

        public decimal? SoilMoistureMin { get; set; }
        public decimal? SoilMoistureMax { get; set; }

        public decimal? LightMin { get; set; }
        public decimal? LightMax { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}