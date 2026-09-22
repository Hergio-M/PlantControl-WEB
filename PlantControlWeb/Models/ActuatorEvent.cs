namespace PlantControlWeb.Models
{
    public class ActuatorEvent
    {
        public long Id { get; set; }

        public long ActuatorId { get; set; }

        public bool State { get; set; }

        public int? Pwm { get; set; }

        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
