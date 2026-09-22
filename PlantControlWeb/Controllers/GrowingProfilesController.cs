using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantControlWeb.Data;
using PlantControlWeb.Models;

namespace PlantControlWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrowingProfilesController : ControllerBase
    {
        private readonly PlantControlDbContext _context;

        public GrowingProfilesController(PlantControlDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var profiles = await _context.GrowingProfiles
                .AsNoTracking()
                .ToListAsync();

            return Ok(profiles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var profile = await _context.GrowingProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profile == null)
                return NotFound();

            return Ok(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Create(GrowingProfile profile)
        {
            profile.CreatedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;

            _context.GrowingProfiles.Add(profile);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = profile.Id },
                profile
            );
        }
    }
}