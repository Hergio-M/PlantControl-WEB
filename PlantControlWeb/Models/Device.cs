namespace PlantControlWeb.Models
{
    public class Device
    {
        public long Id { get; set; }

        public long ProfileId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string DeviceUid { get; set; } = string.Empty;

        public string? FirmwareVersion { get; set; }

        public string Status { get; set; } = "offline";

        public DateTime? LastSeen { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
