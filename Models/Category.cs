using System.ComponentModel.DataAnnotations;

namespace DigitalLibraryApi.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Description { get; set; }
    }
}
