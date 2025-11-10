using System.ComponentModel.DataAnnotations;

namespace DigitalLibraryApi.DTOs
{
    public class CreateBookDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(60, ErrorMessage = "Author name too long.")]
        public string Author { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{3}-\d{10}$", ErrorMessage = "ISBN must follow pattern 000-0000000000.")]
        public string ISBN { get; set; } = string.Empty;

        [Range(1500, 2100, ErrorMessage = "Year must be between 1500 and 2100.")]
        public int Year { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
