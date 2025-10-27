using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class ChangeLanguageRequestDto
    {
        [Required]
        [StringLength(5)]
        public string Language { get; set; } = string.Empty;
    }
}
