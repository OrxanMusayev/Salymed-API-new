using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services.Interfaces;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(IDoctorService doctorService, ILogger<DoctorsController> logger)
        {
            _doctorService = doctorService ?? throw new ArgumentNullException(nameof(doctorService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Bütün həkimləri gətir
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetAllDoctors()
        {
            try
            {
                var doctors = await _doctorService.GetAllDoctorsAsync();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Həkimləri gətirərkən xəta yarandı");
                return StatusCode(500, "Daxili server xətası");
            }
        }

        /// <summary>
        /// Səhifələnmiş həkim siyahısını gətir
        /// </summary>
        [HttpGet("paginated")]
        public async Task<ActionResult<PaginatedResponse<DoctorResponseDto>>> GetPaginatedDoctors([FromQuery] PaginationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _doctorService.GetPaginatedDoctorsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Səhifələnmiş həkimləri gətirərkən xəta yarandı");
                return StatusCode(500, "Daxili server xətası");
            }
        }

        /// <summary>
        /// ID-yə görə həkim gətir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorResponseDto>> GetDoctor(int id)
        {
            try
            {
                var doctor = await _doctorService.GetDoctorByIdAsync(id);

                if (doctor == null)
                {
                    return NotFound($"ID: {id} olan həkim tapılmadı");
                }

                return Ok(doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID: {Id} olan həkimi gətirərkən xəta yarandı", id);
                return StatusCode(500, "Daxili server xətası");
            }
        }

        /// <summary>
        /// Klinikə görə həkimləri gətir
        /// </summary>
        [HttpGet("clinic/{clinicId}")]
        public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetDoctorsByClinic(int clinicId)
        {
            try
            {
                var doctors = await _doctorService.GetDoctorsByClinicIdAsync(clinicId);
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klinik ID: {ClinicId} üçün həkimləri gətirərkən xəta yarandı", clinicId);
                return StatusCode(500, "Daxili server xətası");
            }
        }

        /// <summary>
        /// Aktiv həkimləri gətir
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetActiveDoctors()
        {
            try
            {
                var doctors = await _doctorService.GetActiveDoctorsAsync();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aktiv həkimləri gətirərkən xəta yarandı");
                return StatusCode(500, "Daxili server xətası");
            }
        }

        /// <summary>
        /// Həkim axtarışı
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> SearchDoctors([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest("Axtarış termini boş ola bilməz");
                }

                var doctors = await _doctorService.SearchDoctorsAsync(searchTerm);
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Həkim axtarışında xəta yarandı. Termin: {SearchTerm}", searchTerm);
                return StatusCode(500, "Daxili server xətası");
            }
        }

        /// <summary>
        /// İxtisasa görə həkimləri gətir
        /// </summary>
        [HttpGet("specialty/{specialty}")]
        public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetDoctorsBySpecialty(string specialty)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(specialty))
                {
                    return BadRequest("İxtisas boş ola bilməz");
                }

                var doctors = await _doctorService.GetDoctorsBySpecialtyAsync(specialty);
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İxtisas: {Specialty} üçün həkimləri gətirərkən xəta yarandı", specialty);
                return StatusCode(500, "Daxili server xətası");
            }
        }

        /// <summary>
        /// Yeni həkim yarat (avatar faylı ilə)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DoctorResponseDto>> CreateDoctor([FromForm] CreateDoctorDto createDoctorDto, IFormFile? avatar)
        {
            try
            {
                _logger.LogInformation("CreateDoctor called with: {@CreateDoctorDto}", createDoctorDto);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var doctor = await _doctorService.CreateDoctorAsync(createDoctorDto, avatar);
                return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id }, doctor);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Həkim yaradılarkən business logic xətası");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Həkim yaradılarkən xəta yarandı");
                return StatusCode(500, "Daxili server xətası");
            }
        }

        /// <summary>
        /// Həkimi yenilə (avatar faylı ilə)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<DoctorResponseDto>> UpdateDoctor(int id, [FromForm] UpdateDoctorDto updateDoctorDto, IFormFile? avatar)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var doctor = await _doctorService.UpdateDoctorAsync(id, updateDoctorDto, avatar);

                if (doctor == null)
                {
                    return NotFound($"ID: {id} olan həkim tapılmadı");
                }

                return Ok(doctor);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Həkim yenilənərkən business logic xətası");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID: {Id} olan həkimi yenilənərkən xəta yarandı", id);
                return StatusCode(500, "Daxili server xətası");
            }
        }

        /// <summary>
        /// Həkimi sil
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            try
            {
                var result = await _doctorService.DeleteDoctorAsync(id);

                if (!result)
                {
                    return NotFound($"ID: {id} olan həkim tapılmadı");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID: {Id} olan həkimi silərkən xəta yarandı", id);
                return StatusCode(500, "Daxili server xətası");
            }
        }

        /// <summary>
        /// Həkimin mövcudluğunu yoxla
        /// </summary>
        [HttpHead("{id}")]
        public async Task<IActionResult> DoctorExists(int id)
        {
            try
            {
                var exists = await _doctorService.DoctorExistsAsync(id);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID: {Id} olan həkimin mövcudluğunu yoxlayarkən xəta yarandı", id);
                return StatusCode(500, "Daxili server xətası");
            }
        }
    }
}