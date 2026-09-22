using Microsoft.EntityFrameworkCore;
using PlantControlWeb.Models;

namespace PlantControlWeb.Data
{
    public class PlantControlDbContext : DbContext
    {
        public PlantControlDbContext(DbContextOptions<PlantControlDbContext> options)
            : base(options)
        {
        }
        public DbSet<GrowingProfile> GrowingProfiles { get; set; }
        public DbSet<Device> Devices { get; set; }

        public DbSet<EnvironmentMeasurement> EnvironmentMeasurements { get; set; }

        public DbSet<Actuator> Actuators { get; set; }

        public DbSet<ActuatorEvent> ActuatorEvents { get; set; }

        public DbSet<Alert> Alerts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // GrowingProfile
            modelBuilder.Entity<GrowingProfile>(entity =>
            {
                entity.ToTable("growing_profiles");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasColumnName("name");

                entity.Property(e => e.Description)
                    .HasColumnName("description");

                entity.Property(e => e.TemperatureAirMin)
                    .HasColumnName("temperature_air_min");

                entity.Property(e => e.TemperatureAirMax)
                    .HasColumnName("temperature_air_max");

                entity.Property(e => e.TemperatureSoilMin)
                    .HasColumnName("temperature_soil_min");

                entity.Property(e => e.TemperatureSoilMax)
                    .HasColumnName("temperature_soil_max");

                entity.Property(e => e.HumidityMin)
                    .HasColumnName("humidity_min");

                entity.Property(e => e.HumidityMax)
                    .HasColumnName("humidity_max");

                entity.Property(e => e.SoilMoistureMin)
                    .HasColumnName("soil_moisture_min");

                entity.Property(e => e.SoilMoistureMax)
                    .HasColumnName("soil_moisture_max");

                entity.Property(e => e.LightMin)
                    .HasColumnName("light_min");

                entity.Property(e => e.LightMax)
                    .HasColumnName("light_max");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");
            });

            // Devices
            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("devices");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.ProfileId)
                    .HasColumnName("profile_id");

                entity.Property(e => e.Name)
                    .HasColumnName("name");

                entity.Property(e => e.DeviceUid)
                    .HasColumnName("device_uid");

                entity.Property(e => e.FirmwareVersion)
                    .HasColumnName("firmware_version");

                entity.Property(e => e.Status)
                    .HasColumnName("status");

                entity.Property(e => e.LastSeen)
                    .HasColumnName("last_seen");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");
            });

            // Environment Measurements
            modelBuilder.Entity<EnvironmentMeasurement>(entity =>
            {
                entity.ToTable("environment_measurements");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.DeviceId)
                    .HasColumnName("device_id");

                entity.Property(e => e.RecordedAt)
                    .HasColumnName("recorded_at");

                entity.Property(e => e.TemperatureAir)
                    .HasColumnName("temperature_air");

                entity.Property(e => e.TemperatureSoil)
                    .HasColumnName("temperature_soil");

                entity.Property(e => e.Humidity)
                    .HasColumnName("humidity");

                entity.Property(e => e.SoilMoisture)
                    .HasColumnName("soil_moisture");

                entity.Property(e => e.Light)
                    .HasColumnName("light");
            });

            // Actuators
            modelBuilder.Entity<Actuator>(entity =>
            {
                entity.ToTable("actuators");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.DeviceId)
                    .HasColumnName("device_id");

                entity.Property(e => e.Name)
                    .HasColumnName("name");

                entity.Property(e => e.Type)
                    .HasColumnName("type");

                entity.Property(e => e.Gpio)
                    .HasColumnName("gpio");

                entity.Property(e => e.Enabled)
                    .HasColumnName("enabled");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");
            });

            // Actuator Events
            modelBuilder.Entity<ActuatorEvent>(entity =>
            {
                entity.ToTable("actuator_events");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.ActuatorId)
                    .HasColumnName("actuator_id");

                entity.Property(e => e.State)
                    .HasColumnName("state");

                entity.Property(e => e.Pwm)
                    .HasColumnName("pwm");

                entity.Property(e => e.Reason)
                    .HasColumnName("reason");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");
            });

            // Alerts
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.ToTable("alerts");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.DeviceId)
                    .HasColumnName("device_id");

                entity.Property(e => e.MeasurementId)
                    .HasColumnName("measurement_id");

                entity.Property(e => e.Type)
                    .HasColumnName("type");

                entity.Property(e => e.Severity)
                    .HasColumnName("severity");

                entity.Property(e => e.Message)
                    .HasColumnName("message");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.ResolvedAt)
                    .HasColumnName("resolved_at");
            });
        }
    }
}
