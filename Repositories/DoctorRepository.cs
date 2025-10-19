using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.DTOs;

namespace backend.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            return await _context.Doctors
                .Include(d => d.Clinic)
                .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
                .ThenByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Doctor> doctors, int totalCount)> GetPaginatedAsync(PaginationRequest request)
        {
            var query = _context.Doctors.Include(d => d.Clinic).AsQueryable();

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "firstname" => request.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(d => d.FirstName)
                    : query.OrderByDescending(d => d.FirstName),
                "lastname" => request.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(d => d.LastName)
                    : query.OrderByDescending(d => d.LastName),
                "email" => request.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(d => d.Email)
                    : query.OrderByDescending(d => d.Email),
                "specialty" => request.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(d => d.Specialty)
                    : query.OrderByDescending(d => d.Specialty),
                "createdat" => request.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(d => d.CreatedAt)
                    : query.OrderByDescending(d => d.CreatedAt),
                "updatedat" or _ => request.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(d => d.UpdatedAt ?? d.CreatedAt).ThenBy(d => d.CreatedAt)
                    : query.OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt).ThenByDescending(d => d.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var doctors = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return (doctors, totalCount);
        }

        public async Task<(IEnumerable<Doctor> doctors, int totalCount)> GetPaginatedByClinicIdAsync(PaginationRequest request, Guid clinicId)
        {
            // ✅ ClinicId ile filtreleme
            var query = _context.Doctors
                .Include(d => d.Clinic)
                .Where(d => d.ClinicId == clinicId)
                .AsQueryable();

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "firstname" => request.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(d => d.FirstName)
                    : query.OrderByDescending(d => d.FirstName),
                "lastname" => request.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(d => d.LastName)
                    : query.OrderByDescending(d => d.LastName),
                "email" => request.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(d => d.Email)
                    : query.OrderByDescending(d => d.Email),
                "specialty" => request.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(d => d.Specialty)
                    : query.OrderByDescending(d => d.Specialty),
                "createdat" => request.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(d => d.CreatedAt)
                    : query.OrderByDescending(d => d.CreatedAt),
                "updatedat" or _ => request.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(d => d.UpdatedAt ?? d.CreatedAt).ThenBy(d => d.CreatedAt)
                    : query.OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt).ThenByDescending(d => d.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var doctors = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return (doctors, totalCount);
        }

        public async Task<Doctor?> GetByIdAsync(Guid id)
        {
            return await _context.Doctors
                .Include(d => d.Clinic)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Doctor>> GetByClinicIdAsync(Guid clinicId)
        {
            return await _context.Doctors
                .Where(d => d.ClinicId == clinicId)
                .Include(d => d.Clinic)
                .OrderBy(d => d.FirstName)
                .ThenBy(d => d.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Doctor>> GetActiveAsync()
        {
            return await _context.Doctors
                .Where(d => d.IsActive)
                .Include(d => d.Clinic)
                .OrderBy(d => d.FirstName)
                .ThenBy(d => d.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Doctor>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await GetAllAsync();

            var term = searchTerm.ToLower().Trim();

            return await _context.Doctors
                .Where(d =>
                    d.FirstName.ToLower().Contains(term) ||
                    d.LastName.ToLower().Contains(term) ||
                    d.Email.ToLower().Contains(term) ||
                    d.Specialty.ToLower().Contains(term))
                .Include(d => d.Clinic)
                .OrderBy(d => d.FirstName)
                .ThenBy(d => d.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty)
        {
            return await _context.Doctors
                .Where(d => d.Specialty.ToLower() == specialty.ToLower())
                .Include(d => d.Clinic)
                .OrderBy(d => d.FirstName)
                .ThenBy(d => d.LastName)
                .ToListAsync();
        }

        public async Task<Doctor> CreateAsync(Doctor doctor)
        {
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor));

            doctor.CreatedAt = DateTime.UtcNow;
            doctor.UpdatedAt = null;

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(doctor.Id) ?? doctor;
        }

        public async Task<Doctor?> UpdateAsync(Guid id, Doctor doctor)
        {
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor));

            var existingDoctor = await _context.Doctors.FindAsync(id);
            if (existingDoctor == null)
                return null;

            // Update properties
            existingDoctor.FirstName = doctor.FirstName;
            existingDoctor.LastName = doctor.LastName;
            existingDoctor.Email = doctor.Email;
            existingDoctor.Phone = doctor.Phone;
            existingDoctor.PhoneCountryCode = doctor.PhoneCountryCode;
            existingDoctor.Specialty = doctor.Specialty;
            existingDoctor.YearsExperience = doctor.YearsExperience;
            existingDoctor.Age = doctor.Age;
            existingDoctor.Gender = doctor.Gender;
            existingDoctor.WorkingHours = doctor.WorkingHours;
            existingDoctor.IsActive = doctor.IsActive;
            existingDoctor.AvatarUrl = doctor.AvatarUrl;
            existingDoctor.Rating = doctor.Rating;
            existingDoctor.ClinicId = doctor.ClinicId;
            existingDoctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return false;

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Doctors.AnyAsync(d => d.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
        {
            var query = _context.Doctors.Where(d => d.Email.ToLower() == email.ToLower());

            if (excludeId.HasValue)
                query = query.Where(d => d.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}