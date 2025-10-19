using backend.Models;
using backend.DTOs;

namespace backend.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<Doctor>> GetAllAsync();
        Task<(IEnumerable<Doctor> doctors, int totalCount)> GetPaginatedAsync(PaginationRequest request);
        Task<(IEnumerable<Doctor> doctors, int totalCount)> GetPaginatedByClinicIdAsync(PaginationRequest request, Guid clinicId);
        Task<Doctor?> GetByIdAsync(Guid id);
        Task<IEnumerable<Doctor>> GetByClinicIdAsync(Guid clinicId);
        Task<IEnumerable<Doctor>> GetActiveAsync();
        Task<IEnumerable<Doctor>> SearchAsync(string searchTerm);
        Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty);
        Task<Doctor> CreateAsync(Doctor doctor);
        Task<Doctor?> UpdateAsync(Guid id, Doctor doctor);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> EmailExistsAsync(string email, Guid? excludeId = null);
    }
}