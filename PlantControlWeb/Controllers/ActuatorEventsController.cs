using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantControlWeb.Data;
using PlantControlWeb.Models;

namespace PlantControlWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActuatorEventsController : ControllerBase
    {
        private readonly PlantControlDbContext _context;

        public ActuatorEventsController(PlantControlDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.ActuatorEvents
                .AsNoTracking()
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("actuator/{actuatorId}")]
        public async Task<IActionResult> GetByActuator(long actuatorId)
        {
            var events = await _context.ActuatorEvents
                .AsNoTracking()
                .Where(e => e.ActuatorId == actuatorId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return Ok(events);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ActuatorEvent actuatorEvent)
        {
            actuatorEvent.CreatedAt = DateTime.UtcNow;

            _context.ActuatorEvents.Add(actuatorEvent);

            await _context.SaveChangesAsync();

            return Ok(actuatorEvent);
        }
    }
}
