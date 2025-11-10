using DigitalLibraryApi.Models;



namespace DigitalLibraryApi.Repositories
{
    public class CategoryRepository
    {
        public static List<Category> Categories { get; set; } = new List<Category>
        {
            new Category { Id = 1, Name = "Fiction", Description = "Imaginative narrative works." },
            new Category { Id = 2, Name = "Science", Description = "Books about science and discovery." },
            new Category { Id = 3, Name = "History", Description = "Historical and biographical books." },
            new Category { Id = 4, Name = "Technology", Description = "Programming and computer science." }
        };
    }
}
