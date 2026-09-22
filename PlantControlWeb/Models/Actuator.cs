namespace PlantControlWeb.Models
{
    public class Actuator
    {
        public long Id { get; set; }

        public long DeviceId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public int? Gpio { get; set; }

        public bool Enabled { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
