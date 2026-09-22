namespace PlantControlWeb.Models
{
    public class EnvironmentMeasurement
    {
        public long Id { get; set; }

        public long DeviceId { get; set; }

        public DateTime RecordedAt { get; set; }

        public decimal? TemperatureAir { get; set; }

        public decimal? TemperatureSoil { get; set; }

        public decimal? Humidity { get; set; }

        public decimal? SoilMoisture { get; set; }

        public decimal? Light { get; set; }
    }
}
