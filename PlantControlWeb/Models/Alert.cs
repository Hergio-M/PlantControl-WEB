namespace PlantControlWeb.Models
{
    public class Alert
    {
        public long Id { get; set; }

        public long DeviceId { get; set; }

        public long? MeasurementId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}
