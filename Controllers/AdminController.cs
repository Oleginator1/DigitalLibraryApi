using Microsoft.AspNetCore.Mvc;
using DigitalLibraryApi.Models;
using DigitalLibraryApi.Repositories;
using DigitalLibraryApi.DTOs;

namespace DigitalLibraryApi.Controllers
{
    [ApiController]
    [Route("admin/books")]
    public class AdminBooksController : ControllerBase
    {
        // POST /admin/books → add a new book
        [HttpPost]
        public ActionResult<Book> AddBook([FromBody] CreateBookDto newBookDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newBook = new Book
            {
                Id = BookRepository.Books.Max(b => b.Id) + 1,
                Title = newBookDto.Title,
                Author = newBookDto.Author,
                ISBN = newBookDto.ISBN,
                Year = newBookDto.Year,
                Description = newBookDto.Description ?? string.Empty,
                CategoryId = newBookDto.CategoryId,
                Category = CategoryRepository.Categories.FirstOrDefault(c => c.Id == newBookDto.CategoryId)
            };

            BookRepository.Books.Add(newBook);
            return CreatedAtAction("GetById", "Books", new { id = newBook.Id }, newBook);
        }

        // PUT /admin/books/{id} → edit/update a book
        [HttpPut("{id}")]
        public ActionResult<Book> UpdateBook(int id, [FromBody] UpdateBookDto updatedBookDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var book = BookRepository.Books.FirstOrDefault(b => b.Id == id);
            if (book is null)
                return NotFound();

         
            // Apply updates only if the value is provided
            if (!string.IsNullOrEmpty(updatedBookDto.Title)) book.Title = updatedBookDto.Title;
            if (!string.IsNullOrEmpty(updatedBookDto.Author)) book.Author = updatedBookDto.Author;
            if (!string.IsNullOrEmpty(updatedBookDto.ISBN)) book.ISBN = updatedBookDto.ISBN;
            if (updatedBookDto.Year.HasValue) book.Year = updatedBookDto.Year.Value;
            if (!string.IsNullOrEmpty(updatedBookDto.Description)) book.Description = updatedBookDto.Description;
            if (updatedBookDto.CategoryId.HasValue)
            {
                book.CategoryId = updatedBookDto.CategoryId.Value;
                book.Category = CategoryRepository.Categories.FirstOrDefault(c => c.Id == book.CategoryId);
            }

        
            return Ok(book);
        }

        // DELETE /admin/books/{id} → remove a book
        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            var book = BookRepository.Books.FirstOrDefault(b => b.Id == id);
            if (book is null)
                return NotFound();

            BookRepository.Books.Remove(book);
            return NoContent();
        }
    }
}
