using System.ComponentModel.DataAnnotations;

namespace DigitalLibraryApi.DTOs
{
    public class UpdateBookDto
    {
        [Required]
        public int Id { get; set; }

        [StringLength(100, MinimumLength = 2)]
        public string? Title { get; set; }

        [StringLength(60)]
        public string? Author { get; set; }

        [RegularExpression(@"^\d{3}-\d{10}$")]
        public string? ISBN { get; set; }

        [Range(1500, 2100)]
        public int? Year { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        
        [Range(1, int.MaxValue, ErrorMessage = "If CategoryId is provided, it must be positive.")]
        public int? CategoryId { get; set; }
    }
}
