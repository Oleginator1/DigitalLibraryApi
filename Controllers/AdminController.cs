using CsvHelper;
using DigitalLibraryApi.DTOs;
using DigitalLibraryApi.Middleware;
using DigitalLibraryApi.Models;
using DigitalLibraryApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

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




        [HttpPost("import")]
        public IActionResult ImportBooks(IFormFile file)
        {
            // 1️⃣ Validate file
            var (isValid, errorMessage) = CsvFileValidator.Validate(file);
            if (!isValid)
                return BadRequest(new { message = errorMessage });

            var imported = new List<Book>();
            var errors = new List<object>();
            int totalRows = 0;

            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<CreateBookDto>().ToList();
                totalRows = records.Count;

                foreach (var record in records.Select((dto, index) => new { dto, index }))
                {
                    var validationContext = new ValidationContext(record.dto);
                    var validationResults = new List<ValidationResult>();

                    if (!Validator.TryValidateObject(record.dto, validationContext, validationResults, true))
                    {
                        errors.Add(new
                        {
                            row = record.index + 1,
                            data = record.dto,
                            errors = validationResults.Select(e => e.ErrorMessage).ToList()
                        });
                        continue;
                    }

                    // If valid, map to Book model
                    var newBook = new Book
                    {
                        Id = BookRepository.Books.Max(b => b.Id) + 1,
                        Title = record.dto.Title.ToUpperInvariant(),
                        Author = record.dto.Author,
                        ISBN = record.dto.ISBN,
                        Year = record.dto.Year,
                        Description = record.dto.Description ?? string.Empty,
                        CategoryId = record.dto.CategoryId
                    };

                    imported.Add(newBook);
                    BookRepository.Books.Add(newBook);
                }
            }

            var response = new
            {
                totalRows,
                successful = imported.Count,
                failed = errors.Count,
                errors,
                imported
            };

            return Ok(response);
        }


        [HttpGet("export")]
        public IActionResult ExportBooks([FromQuery] string? title, [FromQuery] string? author)
        {
            // 1️⃣ Filter the data based on query parameters
            var filteredBooks = BookRepository.Books
                .Where(b =>
                    (string.IsNullOrEmpty(title) || b.Title.Contains(title, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(author) || b.Author.Contains(author, StringComparison.OrdinalIgnoreCase))
                )
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

            // 2️⃣ Convert filtered data to CSV
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(filteredBooks);
            writer.Flush();
            memoryStream.Position = 0;

            // 3️⃣ Return file with correct headers for download
            var fileName = $"books_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(memoryStream.ToArray(), "text/csv", fileName);
        }
    }
}
