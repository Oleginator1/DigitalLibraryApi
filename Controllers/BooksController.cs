using Microsoft.AspNetCore.Mvc;
using DigitalLibraryApi.Models;

namespace DigitalLibraryApi.Controllers
{
    [ApiController]           // Marks this class as a Web API controller
    [Route("books")]          // Base route: /books
    public class BooksController : ControllerBase
    {
        // Static in-memory list of 10 books (our "database")
        private static readonly List<Book> books = new()
        {
            new Book { Id = 1, Title = "The Pragmatic Developer", Author = "A. Coder", ISBN = "978-0000000001", Year = 2010, Description = "Practical advice for daily development." },
            new Book { Id = 2, Title = "Patterns in Software", Author = "B. Architect", ISBN = "978-0000000002", Year = 2012, Description = "Design patterns and system design." },
            new Book { Id = 3, Title = "Learning C#", Author = "C. Teacher", ISBN = "978-0000000003", Year = 2018, Description = "Beginner's guide to C# and .NET." },
            new Book { Id = 4, Title = "Advanced .NET", Author = "D. Expert", ISBN = "978-0000000004", Year = 2020, Description = "Deep dive into .NET internals." },
            new Book { Id = 5, Title = "RESTful By Example", Author = "E. Web", ISBN = "978-0000000005", Year = 2015, Description = "How to design and use REST APIs." },
            new Book { Id = 6, Title = "Clean Code Explained", Author = "F. Cleaner", ISBN = "978-0000000006", Year = 2009, Description = "Principles for readable, maintainable code." },
            new Book { Id = 7, Title = "Database Basics", Author = "G. Storage", ISBN = "978-0000000007", Year = 2011, Description = "Relational and NoSQL fundamentals." },
            new Book { Id = 8, Title = "Search Strategies", Author = "H. Index", ISBN = "978-0000000008", Year = 2016, Description = "Techniques to search and filter data." },
            new Book { Id = 9, Title = "UX for Developers", Author = "I. Designer", ISBN = "978-0000000009", Year = 2019, Description = "Design principles for usable interfaces." },
            new Book { Id = 10, Title = "Deploy and Run", Author = "J. Ops", ISBN = "978-0000000010", Year = 2021, Description = "Deployment, CI/CD and production ops." }
        };

        // GET /books  → list all books
        [HttpGet]
        public ActionResult<IEnumerable<Book>> GetAll()
        {
            return Ok(books);
        }

        // GET /books/{id} → book details
        [HttpGet("{id}")]
        public ActionResult<Book> GetById(int id)
        {
            var book = books.FirstOrDefault(b => b.Id == id);
            return book is not null ? Ok(book) : NotFound();
        }
    }
}
