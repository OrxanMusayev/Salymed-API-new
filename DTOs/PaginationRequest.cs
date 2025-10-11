using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class PaginationRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Səhifə nömrəsi 1-dən böyük olmalıdır")]
        public int Page { get; set; } = 1;

        [Range(1, 200, ErrorMessage = "Səhifə ölçüsü 1-200 arasında olmalıdır")]
        public int PageSize { get; set; } = 30;

        public string SortBy { get; set; } = "updatedAt";
        public string SortDirection { get; set; } = "desc"; // asc or desc
    }
}