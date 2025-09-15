using Microsoft.AspNetCore.Mvc;
using DigitalLibraryApi.Models;
using DigitalLibraryApi.Repositories;

namespace DigitalLibraryApi.Controllers
{
    [ApiController]
    [Route("admin/books")]
    public class AdminBooksController : ControllerBase
    {
        // POST /admin/books → add a new book
        [HttpPost]
        public ActionResult<Book> AddBook([FromBody] Book newBook)
        {
          
            newBook.Id = BookRepository.Books.Max(b => b.Id) + 1;
            BookRepository.Books.Add(newBook);

            return CreatedAtAction("GetById", "Books", new { id = newBook.Id }, newBook);
        }

        // PUT /admin/books/{id} → edit/update a book
        [HttpPut("{id}")]
        public ActionResult<Book> UpdateBook(int id, [FromBody] Book updatedBook)
        {
            var book = BookRepository.Books.FirstOrDefault(b => b.Id == id);
            if (book is null) return NotFound();

            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            book.ISBN = updatedBook.ISBN;
            book.Year = updatedBook.Year;
            book.Description = updatedBook.Description;

            return Ok(book);
        }

        // DELETE /admin/books/{id} → remove a book
        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            var book = BookRepository.Books.FirstOrDefault(b => b.Id == id);
            if (book is null) return NotFound();

            BookRepository.Books.Remove(book);
            return NoContent(); 
        }
    }
}
