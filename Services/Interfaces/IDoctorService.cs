using backend.DTOs;
using Microsoft.AspNetCore.Http;

namespace backend.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync();
        Task<PaginatedResponse<DoctorResponseDto>> GetPaginatedDoctorsAsync(PaginationRequest request);
        Task<DoctorResponseDto?> GetDoctorByIdAsync(int id);
        Task<IEnumerable<DoctorResponseDto>> GetDoctorsByClinicIdAsync(int clinicId);
        Task<IEnumerable<DoctorResponseDto>> GetActiveDoctorsAsync();
        Task<IEnumerable<DoctorResponseDto>> SearchDoctorsAsync(string searchTerm);
        Task<IEnumerable<DoctorResponseDto>> GetDoctorsBySpecialtyAsync(string specialty);
        Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorDto createDoctorDto, IFormFile? avatar = null);
        Task<DoctorResponseDto?> UpdateDoctorAsync(int id, UpdateDoctorDto updateDoctorDto, IFormFile? avatar = null);
        Task<bool> DeleteDoctorAsync(int id);
        Task<bool> DoctorExistsAsync(int id);
    }
}