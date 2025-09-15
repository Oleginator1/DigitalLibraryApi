using Microsoft.AspNetCore.Mvc;
using DigitalLibraryApi.Models;
using DigitalLibraryApi.Repositories;

namespace DigitalLibraryApi.Controllers
{
    [ApiController]
    [Route("loans")]
    public class LoansController : ControllerBase
    {
        private static readonly List<Loan> loans = new();
        private static int nextLoanId = 1;

        [HttpGet]
        public ActionResult<IEnumerable<Loan>> GetAll()
        {
            return Ok(loans);
        }

        [HttpGet("{id}")]
        public ActionResult<Loan> GetById(int id)
        {
            var loan = loans.FirstOrDefault(l => l.Id == id);
            return loan is not null ? Ok(loan) : NotFound();
        }

        [HttpPost]
        public ActionResult<Loan> Borrow([FromBody] Loan request)
        {
            // Validate user
            var user = UserRepository.Users.FirstOrDefault(u => u.Id == request.UserId);
            if (user is null) return BadRequest("User does not exist.");

            // Validate book
            var book = BookRepository.Books.FirstOrDefault(b => b.Id == request.BookId);
            if (book is null) return BadRequest("Book does not exist.");

            // Check if book is already borrowed
            var activeLoan = loans.FirstOrDefault(l => l.BookId == request.BookId && l.ReturnDate == null);
            if (activeLoan is not null) return BadRequest("Book is already borrowed.");

            // Create loan
            var loan = new Loan
            {
                Id = nextLoanId++,
                UserId = request.UserId,
                BookId = request.BookId,
                BorrowDate = DateTime.Now,
                ReturnDate = null
            };

            loans.Add(loan);
            return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
        }

        [HttpPut("{id}/return")]
        public ActionResult<Loan> Return(int id)
        {
            var loan = loans.FirstOrDefault(l => l.Id == id);
            if (loan is null) return NotFound();

            if (loan.ReturnDate is not null)
                return BadRequest("Book already returned.");

            loan.ReturnDate = DateTime.Now;
            return Ok(loan);
        }
    }
}
