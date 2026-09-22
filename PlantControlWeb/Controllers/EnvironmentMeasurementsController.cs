using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantControlWeb.Data;
using PlantControlWeb.Models;

namespace PlantControlWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnvironmentMeasurementsController : ControllerBase
    {
        private readonly PlantControlDbContext _context;

        public EnvironmentMeasurementsController(PlantControlDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var measurements = await _context.EnvironmentMeasurements
                .AsNoTracking()
                .OrderByDescending(m => m.RecordedAt)
                .ToListAsync();

            return Ok(measurements);
        }

        [HttpGet("device/{deviceId}")]
        public async Task<IActionResult> GetByDevice(long deviceId,int hours = 24)
        {
            if (hours <= 0)
            {
                return BadRequest("El número de horas debe ser mayor que cero.");
            }

            var from = DateTime.UtcNow.AddHours(-hours);

            var measurements = await _context.EnvironmentMeasurements
                .AsNoTracking()
                .Where(m =>
                    m.DeviceId == deviceId &&
                    m.RecordedAt >= from)
                .OrderBy(m => m.RecordedAt)
                .ToListAsync();

            return Ok(measurements);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            EnvironmentMeasurement measurement)
        {
            measurement.RecordedAt = DateTime.UtcNow;

            _context.EnvironmentMeasurements.Add(measurement);

            await _context.SaveChangesAsync();

            return Ok(measurement);
        }
    }
}