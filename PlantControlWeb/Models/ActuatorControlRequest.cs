namespace PlantControlWeb.Models
{
    public class ActuatorControlRequest
    {
        public bool State { get; set; }
        public int? Pwm { get; set; }
        public string? Reason { get; set; }
    }
}
