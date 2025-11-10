using DigitalLibraryApi.Models;
using DigitalLibraryApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DigitalLibraryApi.Controllers
{
    [ApiController]
    [Route("books")]
    public class BooksController : ControllerBase
    {
        // GET /books
        [HttpGet]
        public ActionResult<object> GetAll()
        {
            var result = BookRepository.Books.Select(b => new
            {
                b.Id,
                b.Title,
                b.Author,
                b.ISBN,
                b.Year,
                b.Description,
                Category = CategoryRepository.Categories.FirstOrDefault(c => c.Id == b.CategoryId)?.Name
            }
            );

            return Ok(result);
        }

        // GET /books/{id}
        [HttpGet("{id:int}")]
        public ActionResult<object> GetById([FromRoute][Range(1, int.MaxValue, ErrorMessage = "Id must be positive.")] int id)
        {
            var book = BookRepository.Books.FirstOrDefault(b => b.Id == id);
            if (book is null)
                return NotFound($"Book with ID {id} not found.");

            var category = CategoryRepository.Categories.FirstOrDefault(c => c.Id == book.CategoryId);

            var result = new
            {
                book.Id,
                book.Title,
                book.Author,
                book.ISBN,
                book.Year,
                book.Description,
                Category = category?.Name
            };

            return Ok(result);

        }

        // GET /books/search?title=...&author=...
        [HttpGet("search")]
        public ActionResult<IEnumerable<Book>> Search(
            [FromQuery, StringLength(50, MinimumLength = 2)] string? title,
            [FromQuery, StringLength(50, MinimumLength = 2)] string? author)
        {
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(author))
                return BadRequest("You must provide at least a title or author.");

            var results = BookRepository.Books
                .Where(b =>
                    (!string.IsNullOrEmpty(title) && b.Title.Contains(title, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(author) && b.Author.Contains(author, StringComparison.OrdinalIgnoreCase)))
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Author,
                    b.ISBN,
                    b.Year,
                    b.Description,
                    Category = CategoryRepository.Categories.FirstOrDefault(c => c.Id == b.CategoryId)?.Name ?? "Uncategorized"
                })
                .ToList();

            return Ok(results);
        }
    }
}
