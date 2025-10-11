using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClinicTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ClinicTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClinicType>>> GetClinicTypes()
        {
            var clinicTypes = await _context.ClinicTypes
                .Where(ct => ct.IsActive)
                .OrderBy(ct => ct.Name)
                .ToListAsync();

            return Ok(clinicTypes);
        }

        // GET: api/ClinicTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClinicType>> GetClinicType(int id)
        {
            var clinicType = await _context.ClinicTypes.FindAsync(id);

            if (clinicType == null)
            {
                return NotFound();
            }

            return Ok(clinicType);
        }
    }
}
