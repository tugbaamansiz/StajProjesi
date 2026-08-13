using Microsoft.AspNetCore.Mvc;
using StajProjesi.API.Data;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClearFeaturesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClearFeaturesController(AppDbContext context)
        {
            _context = context;
        }

        // DELETE: api/ClearFeatures
        [HttpDelete]
        public async Task<IActionResult> ClearAllFeatures()
        {
            _context.Points.RemoveRange(_context.Points);
            _context.Lines.RemoveRange(_context.Lines);
            _context.Polygons.RemoveRange(_context.Polygons);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Tüm geometriler başarıyla silindi."
            });
        }
    }
}