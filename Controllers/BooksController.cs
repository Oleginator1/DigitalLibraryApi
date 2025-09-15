using Microsoft.AspNetCore.Mvc;
using DigitalLibraryApi.Models;
using DigitalLibraryApi.Repositories;

namespace DigitalLibraryApi.Controllers
{
    [ApiController]
    [Route("books")]
    public class BooksController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<Book>> GetAll()
        {
            return Ok(BookRepository.Books);
        }

        [HttpGet("{id}")]
        public ActionResult<Book> GetById(int id)
        {
            var book = BookRepository.Books.FirstOrDefault(b => b.Id == id);
            return book is not null ? Ok(book) : NotFound();
        }

        [HttpGet("search")]
        public ActionResult<IEnumerable<Book>> Search(
            [FromQuery] string? title,
            [FromQuery] string? author)
        {
            var result = BookRepository.Books.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                result = result.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                result = result.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(result);
        }
    }
}
