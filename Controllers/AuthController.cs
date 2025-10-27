using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("Email already exists");
            }

            // Parse phone number to separate country code and number
            var (countryCode, phoneNumber) = ParsePhoneNumber(registerDto.PhoneNumber);

            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                PhoneNumber = phoneNumber,
                PhoneCountryCode = countryCode,
                Role = registerDto.Role ?? "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Don't return password hash
            user.PasswordHash = "";
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPost("register-clinic")]
        public async Task<ActionResult<ClinicRegistrationResponseDto>> RegisterClinic(ClinicRegistrationDto registrationDto)
        {
            // Validate admin email doesn't exist
            if (await _context.Users.AnyAsync(u => u.Email == registrationDto.Admin.Email))
            {
                return BadRequest(new { message = "Admin email already exists" });
            }

            // Validate clinic email doesn't exist (if provided)
            if (!string.IsNullOrEmpty(registrationDto.Clinic.Email) &&
                await _context.Clinics.AnyAsync(c => c.Email == registrationDto.Clinic.Email))
            {
                return BadRequest(new { message = "Clinic email already exists" });
            }

            // Use transaction to ensure both user and clinic are created together
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Parse admin phone number
                var (adminCountryCode, adminPhoneNumber) = ParsePhoneNumber(registrationDto.Admin.Phone, registrationDto.Admin.PhoneCountryCode);

                // Create admin user
                var adminUser = new User
                {
                    FirstName = registrationDto.Admin.FirstName,
                    LastName = registrationDto.Admin.LastName,
                    Email = registrationDto.Admin.Email,
                    PasswordHash = HashPassword(registrationDto.Admin.Password),
                    PhoneNumber = adminPhoneNumber,
                    PhoneCountryCode = adminCountryCode,
                    Role = "ClinicAdmin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();

                // Parse clinic phone number
                var (clinicCountryCode, clinicPhoneNumber) = ParsePhoneNumber(registrationDto.Clinic.Phone, registrationDto.Clinic.PhoneCountryCode);

                // Create clinic
                var clinic = new Clinic
                {
                    Name = registrationDto.Clinic.Name,
                    PhoneNumber = clinicPhoneNumber,
                    PhoneCountryCode = clinicCountryCode,
                    Email = registrationDto.Clinic.Email,
                    Address = registrationDto.Clinic.Address,
                    City = registrationDto.Clinic.City,
                    Type = registrationDto.Clinic.Type,
                    RegistrationCompleted = true,
                    OwnerId = adminUser.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Clinics.Add(clinic);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Generate real JWT token
                var token = _jwtService.GenerateToken(adminUser, clinic.Id);

                // Return response with created data
                return Ok(new ClinicRegistrationResponseDto
                {
                    UserId = adminUser.Id,
                    ClinicId = clinic.Id,
                    Email = adminUser.Email,
                    FirstName = adminUser.FirstName,
                    LastName = adminUser.LastName,
                    ClinicName = clinic.Name,
                    Role = adminUser.Role,
                    PreferredLanguage = adminUser.PreferredLanguage,
                    Token = token,
                    Message = "Clinic registration successful"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            
            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password");
            }

            if (!user.IsActive)
            {
                return Unauthorized("Account is deactivated");
            }

            // Check if user is a clinic admin and get clinic info
            Guid? clinicId = null;
            string? clinicName = null;
            
            if (user.Role == "ClinicAdmin")
            {
                var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.OwnerId == user.Id);
                if (clinic != null)
                {
                    clinicId = clinic.Id;
                    clinicName = clinic.Name;
                }
            }

            // Generate real JWT token
            var token = _jwtService.GenerateToken(user, clinicId);

            return Ok(new LoginResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                PreferredLanguage = user.PreferredLanguage,
                ClinicId = clinicId,
                ClinicName = clinicName,
                Token = token
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            // Don't return password hash
            user.PasswordHash = "";
            return user;
        }
        [HttpPost("logout")]
        [Authorize] // Requires authentication
        public IActionResult Logout()
        {
            // Get current user from JWT token
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            
            // Log the logout event
            Console.WriteLine($"User logout: {userEmail} (ID: {userId})");
            
            // JWT kullanıldığında logout genelde client-side'da token silmektir
            // Burada ek işlemler yapılabilir:
            // 1. Token'ı blacklist'e ekle (Redis gibi)
            // 2. User activity log kaydet
            // 3. Session bilgilerini temizle
            
            return Ok(new { message = "Logout successful", userEmail });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        /// <summary>
        /// Parse phone number string and separate country code from the number.
        /// Supports formats like "+994501234567" or just "501234567" with separate country code.
        /// </summary>
        private (string countryCode, string phoneNumber) ParsePhoneNumber(string? fullPhone, string? providedCountryCode = null)
        {
            if (string.IsNullOrWhiteSpace(fullPhone))
            {
                return (providedCountryCode ?? "+994", "");
            }

            fullPhone = fullPhone.Trim();

            // If country code is provided separately, use it
            if (!string.IsNullOrWhiteSpace(providedCountryCode))
            {
                // Remove country code from fullPhone if it starts with it
                if (fullPhone.StartsWith(providedCountryCode))
                {
                    fullPhone = fullPhone.Substring(providedCountryCode.Length).Trim();
                }
                return (providedCountryCode, fullPhone);
            }

            // Try to extract country code from the full phone number
            if (fullPhone.StartsWith("+"))
            {
                // Extract country code (+ followed by 1-4 digits)
                var match = System.Text.RegularExpressions.Regex.Match(fullPhone, @"^(\+\d{1,4})(.*)$");
                if (match.Success)
                {
                    return (match.Groups[1].Value, match.Groups[2].Value.Trim());
                }
            }

            // Default to Azerbaijan if no country code found
            return ("+994", fullPhone);
        }
    }

    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string PreferredLanguage { get; set; } = "az";
        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public string Token { get; set; } = string.Empty;
    }

    public class ClinicRegistrationDto
    {
        public ClinicDataDto Clinic { get; set; } = new ClinicDataDto();
        public AdminDataDto Admin { get; set; } = new AdminDataDto();
    }

    public class ClinicDataDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? PhoneCountryCode { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? City { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class AdminDataDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? PhoneCountryCode { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public class ClinicRegistrationResponseDto
    {
        public Guid UserId { get; set; }
        public Guid ClinicId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string PreferredLanguage { get; set; } = "az";
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
