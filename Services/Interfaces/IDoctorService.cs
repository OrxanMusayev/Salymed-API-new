using backend.DTOs;
using Microsoft.AspNetCore.Http;

namespace backend.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync();
        Task<PaginatedResponse<DoctorResponseDto>> GetPaginatedDoctorsAsync(PaginationRequest request);
        Task<PaginatedResponse<DoctorResponseDto>> GetPaginatedDoctorsByClinicIdAsync(PaginationRequest request, Guid clinicId);
        Task<DoctorResponseDto?> GetDoctorByIdAsync(Guid id);
        Task<IEnumerable<DoctorResponseDto>> GetDoctorsByClinicIdAsync(Guid clinicId);
        Task<IEnumerable<DoctorResponseDto>> GetActiveDoctorsAsync();
        Task<IEnumerable<DoctorResponseDto>> SearchDoctorsAsync(string searchTerm);
        Task<IEnumerable<DoctorResponseDto>> GetDoctorsBySpecialtyAsync(string specialty);
        Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorDto createDoctorDto, IFormFile? avatar = null);
        Task<DoctorResponseDto?> UpdateDoctorAsync(Guid id, UpdateDoctorDto updateDoctorDto, IFormFile? avatar = null);
        Task<bool> DeleteDoctorAsync(Guid id);
        Task<bool> DoctorExistsAsync(Guid id);
    }
}