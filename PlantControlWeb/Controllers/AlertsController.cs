using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantControlWeb.Data;
using PlantControlWeb.Models;

namespace PlantControlWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly PlantControlDbContext _context;

        public AlertsController(PlantControlDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var alerts = await _context.Alerts
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(alerts);
        }

        [HttpGet("device/{deviceId}")]
        public async Task<IActionResult> GetByDevice(long deviceId)
        {
            var alerts = await _context.Alerts
                .AsNoTracking()
                .Where(a => a.DeviceId == deviceId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(alerts);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Alert alert)
        {
            alert.CreatedAt = DateTime.UtcNow;

            _context.Alerts.Add(alert);

            await _context.SaveChangesAsync();

            return Ok(alert);
        }
    }
}
