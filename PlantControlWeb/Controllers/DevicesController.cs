using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantControlWeb.Data;
using PlantControlWeb.Models;
using PlantControlWeb.Services;

namespace PlantControlWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly PlantControlDbContext _context;
        private readonly EnvironmentEvaluationService _environmentEvaluationService;

        public DevicesController(
            PlantControlDbContext context,
            EnvironmentEvaluationService environmentEvaluationService)
        {
            _context = context;
            _environmentEvaluationService = environmentEvaluationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var devices = await _context.Devices
                .AsNoTracking()
                .ToListAsync();

            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var device = await _context.Devices
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
                return NotFound();

            return Ok(device);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Device device)
        {
            var now = DateTime.UtcNow;

            device.CreatedAt = now;
            device.UpdatedAt = now;

            if (device.Status == "online")
            {
                device.LastSeen = now;
            }

            _context.Devices.Add(device);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = device.Id },
                device
            );
        }

        [HttpGet("{id}/dashboard")]
        public async Task<IActionResult> GetDashboard(long id)
        {
            var device = await _context.Devices
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
                return NotFound("El dispositivo no existe.");

            // --------------------------------------------------
            // ESTADO DEL DISPOSITIVO
            // --------------------------------------------------

            var now = DateTime.UtcNow;

            string status;

            if (!device.LastSeen.HasValue)
            {
                status = "offline";
            }
            else
            {
                var elapsed = now - device.LastSeen.Value;

                if (elapsed.TotalSeconds < 30)
                {
                    status = "online";
                }
                else if (elapsed.TotalSeconds <= 60)
                {
                    status = "warning";
                }
                else
                {
                    status = "offline";
                }
            }

            // --------------------------------------------------
            // PERFIL DE CULTIVO
            // --------------------------------------------------

            var profile = await _context.GrowingProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == device.ProfileId);

            // --------------------------------------------------
            // ÚLTIMA MEDICIÓN
            // --------------------------------------------------

            var measurement = await _context.EnvironmentMeasurements
                .AsNoTracking()
                .Where(m => m.DeviceId == id)
                .OrderByDescending(m => m.RecordedAt)
                .FirstOrDefaultAsync();

            // --------------------------------------------------
            // ACTUADORES
            // --------------------------------------------------

            var actuators = await _context.Actuators
                .AsNoTracking()
                .Where(a => a.DeviceId == id)
                .ToListAsync();

            var actuatorIds = actuators
                .Select(a => a.Id)
                .ToList();

            var latestActuatorEvents = await _context.ActuatorEvents
                .AsNoTracking()
                .Where(e => actuatorIds.Contains(e.ActuatorId))
                .GroupBy(e => e.ActuatorId)
                .Select(g => g
                    .OrderByDescending(e => e.CreatedAt)
                    .First())
                .ToListAsync();

            // --------------------------------------------------
            // CONSTRUIR ESTADO DE ACTUADORES
            // --------------------------------------------------

            var actuatorStatus = actuators.Select(actuator =>
            {
                var lastEvent = latestActuatorEvents
                    .FirstOrDefault(e => e.ActuatorId == actuator.Id);

                return new
                {
                    id = actuator.Id,
                    name = actuator.Name,
                    type = actuator.Type,
                    gpio = actuator.Gpio,
                    enabled = actuator.Enabled,
                    state = lastEvent?.State ?? false,
                    pwm = lastEvent?.Pwm,
                    reason = lastEvent?.Reason,
                    lastUpdated = lastEvent?.CreatedAt
                };
            }).ToList();

            // --------------------------------------------------
            // ALERTAS ACTIVAS
            // --------------------------------------------------

            var alerts = await _context.Alerts
                .AsNoTracking()
                .Where(a =>
                    a.DeviceId == id &&
                    a.ResolvedAt == null)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // --------------------------------------------------
            // RESPUESTA DEL DASHBOARD
            // --------------------------------------------------

            return Ok(new
            {
                device = new
                {
                    id = device.Id,
                    profileId = device.ProfileId,
                    name = device.Name,
                    deviceUid = device.DeviceUid,
                    firmwareVersion = device.FirmwareVersion,
                    status,
                    lastSeen = device.LastSeen,
                    secondsSinceLastSeen = device.LastSeen.HasValue
                        ? Math.Round(
                            (now - device.LastSeen.Value).TotalSeconds,
                            1)
                        : (double?)null,
                    createdAt = device.CreatedAt,
                    updatedAt = device.UpdatedAt
                },

                profile,

                environment = measurement,

                actuators = actuatorStatus,

                alerts
            });
        }

        [HttpPost("{id}/telemetry")]
        public async Task<IActionResult> ReceiveTelemetry(
    long id,
    TelemetryRequest request)
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
                return NotFound("El dispositivo no existe.");

            var now = DateTime.UtcNow;

            var measurement = new EnvironmentMeasurement
            {
                DeviceId = id,
                RecordedAt = now,
                TemperatureAir = request.TemperatureAir,
                TemperatureSoil = request.TemperatureSoil,
                Humidity = request.Humidity,
                SoilMoisture = request.SoilMoisture,
                Light = request.Light
            };

            _context.EnvironmentMeasurements.Add(measurement);

            device.LastSeen = now;
            device.Status = "online";
            device.UpdatedAt = now;

            await _context.SaveChangesAsync();

            await _environmentEvaluationService.EvaluateAsync(
                device,
                measurement);

            return Ok(new
            {
                message = "Telemetría recibida correctamente.",
                deviceId = device.Id,
                recordedAt = measurement.RecordedAt,
                measurement
            });
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetStatus(long id)
        {
            var device = await _context.Devices
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
                return NotFound("El dispositivo no existe.");

            var now = DateTime.UtcNow;

            string status;

            if (!device.LastSeen.HasValue)
            {
                status = "offline";
            }
            else
            {
                var elapsed = now - device.LastSeen.Value;

                if (elapsed.TotalSeconds < 30)
                {
                    status = "online";
                }
                else if (elapsed.TotalSeconds <= 60)
                {
                    status = "warning";
                }
                else
                {
                    status = "offline";
                }
            }

            return Ok(new
            {
                deviceId = device.Id,
                name = device.Name,
                deviceUid = device.DeviceUid,
                status,
                lastSeen = device.LastSeen,
                secondsSinceLastSeen = device.LastSeen.HasValue
                    ? Math.Round((now - device.LastSeen.Value).TotalSeconds, 1)
                    : (double?)null
            });
        }
    }
}
