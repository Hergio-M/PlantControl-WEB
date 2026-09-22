using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantControlWeb.Data;
using PlantControlWeb.Models;

namespace PlantControlWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActuatorsController : ControllerBase
    {
        private readonly PlantControlDbContext _context;

        public ActuatorsController(PlantControlDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var actuators = await _context.Actuators
                .AsNoTracking()
                .ToListAsync();

            return Ok(actuators);
        }

        [HttpGet("device/{deviceId}")]
        public async Task<IActionResult> GetByDevice(long deviceId)
        {
            var actuators = await _context.Actuators
                .AsNoTracking()
                .Where(a => a.DeviceId == deviceId)
                .ToListAsync();

            return Ok(actuators);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Actuator actuator)
        {
            var now = DateTime.UtcNow;

            actuator.CreatedAt = now;
            actuator.UpdatedAt = now;

            _context.Actuators.Add(actuator);

            await _context.SaveChangesAsync();

            return Ok(actuator);
        }

        [HttpPost("{id}/control")]
        public async Task<IActionResult> Control(
    long id,
    ActuatorControlRequest request)
        {
            var actuator = await _context.Actuators
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actuator == null)
                return NotFound();

            if (!actuator.Enabled)
                return BadRequest("El actuador está deshabilitado.");

            if (request.Pwm.HasValue &&
                (request.Pwm < 0 || request.Pwm > 100))
            {
                return BadRequest("El PWM debe estar entre 0 y 100.");
            }

            var actuatorEvent = new ActuatorEvent
            {
                ActuatorId = actuator.Id,
                State = request.State,
                Pwm = request.Pwm,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow
            };

            _context.ActuatorEvents.Add(actuatorEvent);

            await _context.SaveChangesAsync();

            return Ok(actuatorEvent);
        }

        [HttpGet("device/{deviceId}/status")]
        public async Task<IActionResult> GetStatus(long deviceId)
        {
            var actuators = await _context.Actuators
                .AsNoTracking()
                .Where(a => a.DeviceId == deviceId)
                .ToListAsync();

            var result = new List<object>();

            foreach (var actuator in actuators)
            {
                var lastEvent = await _context.ActuatorEvents
                    .AsNoTracking()
                    .Where(e => e.ActuatorId == actuator.Id)
                    .OrderByDescending(e => e.CreatedAt)
                    .FirstOrDefaultAsync();

                result.Add(new
                {
                    actuatorId = actuator.Id,
                    name = actuator.Name,
                    type = actuator.Type,
                    gpio = actuator.Gpio,
                    enabled = actuator.Enabled,
                    state = lastEvent?.State ?? false,
                    pwm = lastEvent?.Pwm,
                    lastUpdated = lastEvent?.CreatedAt
                });
            }

            return Ok(result);
        }
    }
}