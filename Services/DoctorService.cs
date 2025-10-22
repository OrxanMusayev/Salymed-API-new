using backend.DTOs;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace backend.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly string _avatarUploadPath;

        public DoctorService(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _avatarUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");

            // Avatar klasörünü oluştur
            if (!Directory.Exists(_avatarUploadPath))
            {
                Directory.CreateDirectory(_avatarUploadPath);
            }
        }

        public async Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync()
        {
            var doctors = await _doctorRepository.GetAllAsync();
            return doctors.Select(MapToDto);
        }

        public async Task<PaginatedResponse<DoctorResponseDto>> GetPaginatedDoctorsAsync(PaginationRequest request)
        {
            var (doctors, totalCount) = await _doctorRepository.GetPaginatedAsync(request);

            return new PaginatedResponse<DoctorResponseDto>
            {
                Data = doctors.Select(MapToDto),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        public async Task<PaginatedResponse<DoctorResponseDto>> GetPaginatedDoctorsByClinicIdAsync(PaginationRequest request, Guid clinicId)
        {
            var (doctors, totalCount) = await _doctorRepository.GetPaginatedByClinicIdAsync(request, clinicId);

            return new PaginatedResponse<DoctorResponseDto>
            {
                Data = doctors.Select(MapToDto),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        public async Task<DoctorResponseDto?> GetDoctorByIdAsync(Guid id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);
            return doctor != null ? MapToDto(doctor) : null;
        }

        public async Task<IEnumerable<DoctorResponseDto>> GetDoctorsByClinicIdAsync(Guid clinicId)
        {
            var doctors = await _doctorRepository.GetByClinicIdAsync(clinicId);
            return doctors.Select(MapToDto);
        }

        public async Task<IEnumerable<DoctorResponseDto>> GetActiveDoctorsAsync()
        {
            var doctors = await _doctorRepository.GetActiveAsync();
            return doctors.Select(MapToDto);
        }

        public async Task<IEnumerable<DoctorResponseDto>> SearchDoctorsAsync(string searchTerm)
        {
            var doctors = await _doctorRepository.SearchAsync(searchTerm);
            return doctors.Select(MapToDto);
        }

        public async Task<IEnumerable<DoctorResponseDto>> GetDoctorsBySpecialtyAsync(string specialty)
        {
            var doctors = await _doctorRepository.GetBySpecialtyAsync(specialty);
            return doctors.Select(MapToDto);
        }

        public async Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorDto createDoctorDto, IFormFile? avatar = null)
        {
            // Business logic validation
            if (await _doctorRepository.EmailExistsAsync(createDoctorDto.Email))
            {
                throw new InvalidOperationException("Bu e-mail ünvanı ilə həkim artıq mövcuddur.");
            }

            var doctor = MapFromCreateDto(createDoctorDto);

            // Avatar faylını yükle
            if (avatar != null)
            {
                doctor.AvatarUrl = await SaveAvatarAsync(avatar);
            }

            var createdDoctor = await _doctorRepository.CreateAsync(doctor);

            return MapToDto(createdDoctor);
        }

        public async Task<DoctorResponseDto?> UpdateDoctorAsync(Guid id, UpdateDoctorDto updateDoctorDto, IFormFile? avatar = null)
        {
            // Business logic validation
            if (!await _doctorRepository.ExistsAsync(id))
            {
                return null;
            }

            if (await _doctorRepository.EmailExistsAsync(updateDoctorDto.Email, id))
            {
                throw new InvalidOperationException("Bu e-mail ünvanı ilə başqa həkim mövcuddur.");
            }

            var doctor = MapFromUpdateDto(updateDoctorDto);

            // Avatar faylını yükle
            if (avatar != null)
            {
                doctor.AvatarUrl = await SaveAvatarAsync(avatar);
            }

            var updatedDoctor = await _doctorRepository.UpdateAsync(id, doctor);

            return updatedDoctor != null ? MapToDto(updatedDoctor) : null;
        }

        public async Task<bool> DeleteDoctorAsync(Guid id)
        {
            if (!await _doctorRepository.ExistsAsync(id))
            {
                return false;
            }

            return await _doctorRepository.DeleteAsync(id);
        }

        public async Task<bool> DoctorExistsAsync(Guid id)
        {
            return await _doctorRepository.ExistsAsync(id);
        }

        // Mapping methods
        private static DoctorResponseDto MapToDto(Doctor doctor)
        {
            return new DoctorResponseDto
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Email = doctor.Email,
                Phone = doctor.Phone,
                PhoneCountryCode = doctor.PhoneCountryCode,
                Specialty = doctor.Specialty,
                YearsExperience = doctor.YearsExperience,
                Age = doctor.Age,
                Gender = doctor.Gender,
                WorkingHours = doctor.WorkingHours,
                IsActive = doctor.IsActive,
                AvatarUrl = doctor.AvatarUrl,
                Rating = doctor.Rating,
                CreatedAt = doctor.CreatedAt,
                UpdatedAt = doctor.UpdatedAt,
                ClinicId = doctor.ClinicId,
                ClinicName = doctor.Clinic?.Name
            };
        }

        private static Doctor MapFromCreateDto(CreateDoctorDto dto)
        {
            return new Doctor
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                PhoneCountryCode = dto.PhoneCountryCode,
                Specialty = dto.Specialty,
                YearsExperience = dto.YearsExperience,
                Age = dto.Age,
                Gender = dto.Gender,
                WorkingHours = dto.WorkingHours,
                IsActive = dto.IsActive,
                AvatarUrl = dto.AvatarUrl,
                Rating = (float)dto.Rating,
                ClinicId = dto.ClinicId
            };
        }

        private async Task<string> SaveAvatarAsync(IFormFile avatar)
        {
            // Dosya uzantısını kontrol et
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(avatar.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException("Sadece JPG, JPEG, PNG ve GIF dosyaları yüklənə bilər.");
            }

            // Dosya boyutunu kontrol et (5MB max)
            if (avatar.Length > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException("Avatar faylının ölçüsü 5MB-dan çox ola bilməz.");
            }

            // Benzersiz dosya adı oluştur
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_avatarUploadPath, uniqueFileName);

            // Dosyayı kaydet
            using var stream = new FileStream(filePath, FileMode.Create);
            await avatar.CopyToAsync(stream);

            // URL'i döndür
            return $"/avatars/{uniqueFileName}";
        }

        private static Doctor MapFromUpdateDto(UpdateDoctorDto dto)
        {
            return new Doctor
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                PhoneCountryCode = dto.PhoneCountryCode,
                Specialty = dto.Specialty,
                YearsExperience = dto.YearsExperience,
                Age = dto.Age,
                Gender = dto.Gender,
                WorkingHours = dto.WorkingHours,
                IsActive = dto.IsActive,
                AvatarUrl = dto.AvatarUrl,
                Rating = (float)dto.Rating,
                ClinicId = dto.ClinicId
            };
        }
    }
}