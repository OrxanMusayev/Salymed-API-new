using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Gets the current user's ID from JWT token claims
        /// </summary>
        /// <returns>User ID or null if not found</returns>
        protected Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Gets the current user's clinic ID from JWT token claims
        /// </summary>
        /// <returns>Clinic ID or null if not found</returns>
        protected Guid? GetCurrentClinicId()
        {
            var clinicIdClaim = User.FindFirst("ClinicId");
            if (clinicIdClaim != null && Guid.TryParse(clinicIdClaim.Value, out var clinicId))
            {
                return clinicId;
            }
            return null;
        }

        /// <summary>
        /// Gets the current user's email from JWT token claims
        /// </summary>
        /// <returns>Email or null if not found</returns>
        protected string? GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Gets the current user's role from JWT token claims
        /// </summary>
        /// <returns>Role or null if not found</returns>
        protected string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Gets the current user's full name from JWT token claims
        /// </summary>
        /// <returns>Full name or null if not found</returns>
        protected string? GetCurrentUserFullName()
        {
            var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value;
            var lastName = User.FindFirst(ClaimTypes.Surname)?.Value;

            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                return $"{firstName} {lastName}";
            }

            return firstName ?? lastName;
        }

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        /// <param name="role">Role to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        protected bool HasRole(string role)
        {
            return User.IsInRole(role);
        }

        /// <summary>
        /// Checks if the current user is a clinic admin
        /// </summary>
        /// <returns>True if user is a clinic admin, false otherwise</returns>
        protected bool IsClinicAdmin()
        {
            return HasRole("ClinicAdmin");
        }

        /// <summary>
        /// Checks if the current user is a system admin
        /// </summary>
        /// <returns>True if user is a system admin, false otherwise</returns>
        protected bool IsSystemAdmin()
        {
            return HasRole("SystemAdmin");
        }

        /// <summary>
        /// Validates that the user has a clinic ID (for clinic-specific operations)
        /// </summary>
        /// <returns>ActionResult with BadRequest if no clinic ID, null otherwise</returns>
        protected ActionResult? ValidateClinicAccess()
        {
            var clinicId = GetCurrentClinicId();
            if (!clinicId.HasValue)
            {
                return BadRequest(new { message = "User has no associated clinic" });
            }
            return null;
        }

        /// <summary>
        /// Gets the current clinic ID and validates access
        /// </summary>
        /// <returns>Tuple with clinic ID and potential error response</returns>
        protected (Guid? clinicId, ActionResult? error) GetAndValidateClinicId()
        {
            var clinicId = GetCurrentClinicId();
            if (!clinicId.HasValue)
            {
                return (null, BadRequest(new { message = "User has no associated clinic" }));
            }
            return (clinicId, null);
        }
    }
}
