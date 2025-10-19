using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClinicsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/clinics
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Clinic>>> GetClinics()
        {
            return await _context.Clinics
                .Where(c => c.IsActive)
                .Include(c => c.Owner)
                .ToListAsync();
        }

        // GET: api/clinics/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Clinic>> GetClinic(Guid id)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinic == null)
            {
                return NotFound();
            }

            return clinic;
        }

        // POST: api/clinics
        [HttpPost]
        public async Task<ActionResult<Clinic>> PostClinic(CreateClinicDto clinicDto)
        {
            var clinic = new Clinic
            {
                Name = clinicDto.Name,
                Description = clinicDto.Description,
                Address = clinicDto.Address,
                PhoneNumber = clinicDto.PhoneNumber,
                PhoneCountryCode = clinicDto.PhoneCountryCode,
                Type = clinicDto.Type,
                Email = clinicDto.Email,
                Website = clinicDto.Website,
                City = clinicDto.City,
                State = clinicDto.State,
                ZipCode = clinicDto.ZipCode,
                OwnerId = clinicDto.OwnerId
            };

            _context.Clinics.Add(clinic);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClinic), new { id = clinic.Id }, clinic);
        }

        // PUT: api/clinics/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClinic(Guid id, UpdateClinicDto clinicDto)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
            {
                return NotFound();
            }

            clinic.Name = clinicDto.Name;
            clinic.Description = clinicDto.Description;
            clinic.Address = clinicDto.Address;
            clinic.PhoneNumber = clinicDto.PhoneNumber;
            clinic.PhoneCountryCode = clinicDto.PhoneCountryCode;
            clinic.Type = clinicDto.Type;
            clinic.Email = clinicDto.Email;
            clinic.Website = clinicDto.Website;
            clinic.City = clinicDto.City;
            clinic.State = clinicDto.State;
            clinic.ZipCode = clinicDto.ZipCode;
            clinic.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClinicExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/clinics/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClinic(Guid id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
            {
                return NotFound();
            }

            clinic.IsActive = false;
            clinic.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/clinics/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Clinic>>> GetClinicsByUser(Guid userId)
        {
            return await _context.Clinics
                .Where(c => c.OwnerId == userId && c.IsActive)
                .ToListAsync();
        }

        // GET: api/clinics/info - Get current clinic info (assumes single clinic setup)
        [HttpGet("info")]
        public async Task<ActionResult<Clinic>> GetClinicInfo()
        {
            var clinic = await _context.Clinics
                .Where(c => c.IsActive)
                .FirstOrDefaultAsync();

            if (clinic == null)
            {
                // Create a default clinic if none exists
                clinic = new Clinic
                {
                    Name = "Salymed Tibb Mərkəzi",
                    Type = "general",
                    PhoneNumber = "125550102",
                    PhoneCountryCode = "+994",
                    Email = "info@salymed.az",
                    Address = "Nizami küç. 123, Yasamal rayonu",
                    City = "Bakı",
                    Website = "https://www.salymed.az"
                };

                _context.Clinics.Add(clinic);
                await _context.SaveChangesAsync();
            }

            return clinic;
        }

        // PUT: api/clinics/info - Update current clinic info
        [HttpPut("info")]
        public async Task<ActionResult<Clinic>> UpdateClinicInfo(UpdateClinicDto clinicDto)
        {
            var clinic = await _context.Clinics
                .Where(c => c.IsActive)
                .FirstOrDefaultAsync();

            if (clinic == null)
            {
                return NotFound("Klinik məlumatları tapılmadı");
            }

            clinic.Name = clinicDto.Name;
            clinic.Description = clinicDto.Description;
            clinic.Address = clinicDto.Address;
            clinic.PhoneNumber = clinicDto.PhoneNumber;
            clinic.PhoneCountryCode = clinicDto.PhoneCountryCode;
            clinic.Type = clinicDto.Type;
            clinic.Email = clinicDto.Email;
            clinic.Website = clinicDto.Website;
            clinic.City = clinicDto.City;
            clinic.State = clinicDto.State;
            clinic.ZipCode = clinicDto.ZipCode;
            clinic.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return clinic;
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        private bool ClinicExists(Guid id)
        {
            return _context.Clinics.Any(e => e.Id == id);
        }
    }

    public class CreateClinicDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? PhoneCountryCode { get; set; }
        public string? Type { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public Guid? OwnerId { get; set; }
    }

    public class UpdateClinicDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? PhoneCountryCode { get; set; }
        public string? Type { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
    }
}