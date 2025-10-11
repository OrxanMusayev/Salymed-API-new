using backend.Models;
using backend.DTOs;

namespace backend.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<Doctor>> GetAllAsync();
        Task<(IEnumerable<Doctor> doctors, int totalCount)> GetPaginatedAsync(PaginationRequest request);
        Task<Doctor?> GetByIdAsync(int id);
        Task<IEnumerable<Doctor>> GetByClinicIdAsync(int clinicId);
        Task<IEnumerable<Doctor>> GetActiveAsync();
        Task<IEnumerable<Doctor>> SearchAsync(string searchTerm);
        Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty);
        Task<Doctor> CreateAsync(Doctor doctor);
        Task<Doctor?> UpdateAsync(int id, Doctor doctor);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    }
}