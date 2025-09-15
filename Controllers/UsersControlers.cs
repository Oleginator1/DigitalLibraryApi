using Microsoft.AspNetCore.Mvc;
using DigitalLibraryApi.Models;

namespace DigitalLibraryApi.Controllers
{
    [ApiController]
    [Route("users")] 
    public class UsersController : ControllerBase
    {
        
        private static readonly List<User> users = new()
        {
            new User { Id = 1, Username = "alice", Email = "alice@example.com" },
            new User { Id = 2, Username = "bob", Email = "bob@example.com" },
            new User { Id = 3, Username = "charlie", Email = "charlie@example.com" }
        };

        // GET /users → list all users
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAll()
        {
            return Ok(users);
        }

        // GET /users/{id} → user details
        [HttpGet("{id}")]
        public ActionResult<User> GetById(int id)
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            return user is not null ? Ok(user) : NotFound();
        }
    }
}
