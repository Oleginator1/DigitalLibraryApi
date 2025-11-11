using CsvHelper;
using DigitalLibraryApi.DTOs;
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
        [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB limit
        public async Task<IActionResult> ImportBooks(IFormFile file)
        {
            // ✅ 1. Validate file presence
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // ✅ 2. Validate MIME type
            if (file.ContentType != "text/csv" && file.ContentType != "application/vnd.ms-excel")
                return BadRequest("Invalid file type. Only CSV files are accepted.");

            // ✅ 3. Validate file size
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("File too large. Maximum allowed size is 5 MB.");

            var result = new
            {
                totalRows = 0,
                successful = 0,
                failed = 0,
                errors = new List<object>(),
                imported = new List<Book>()
            };

            using var stream = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(stream, CultureInfo.InvariantCulture);

            var records = new List<CreateBookDto>();

            try
            {
                records = csv.GetRecords<CreateBookDto>().ToList();
            }
            catch (Exception)
            {
                return BadRequest("CSV file format is invalid or columns don’t match CreateBookDto.");
            }

            var totalRows = records.Count;
            var importedBooks = new List<Book>();
            var errorList = new List<object>();

            // ✅ 4. Validate and process each row
            foreach (var (record, index) in records.Select((r, i) => (r, i + 1)))
            {
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(record);

                if (!Validator.TryValidateObject(record, validationContext, validationResults, true))
                {
                    errorList.Add(new
                    {
                        row = index,
                        data = record,
                        errors = validationResults.Select(v => v.ErrorMessage).ToList()
                    });
                    continue;
                }

                // ✅ 5. Map to Book model
                var newBook = new Book
                {
                    Id = BookRepository.Books.Max(b => b.Id) + 1,
                    Title = record.Title,
                    Author = record.Author,
                    ISBN = record.ISBN,
                    Year = record.Year,
                    Description = record.Description ?? string.Empty,
                    CategoryId = record.CategoryId,
                    Category = CategoryRepository.Categories.FirstOrDefault(c => c.Id == record.CategoryId)
                };

                importedBooks.Add(newBook);
                BookRepository.Books.Add(newBook);
            }

            // ✅ 6. Build final JSON response
            var response = new
            {
                totalRows = totalRows,
                successful = importedBooks.Count,
                failed = errorList.Count,
                errors = errorList,
                imported = importedBooks
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
