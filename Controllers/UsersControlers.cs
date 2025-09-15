using Microsoft.AspNetCore.Mvc;
using DigitalLibraryApi.Models;
using DigitalLibraryApi.Repositories;

namespace DigitalLibraryApi.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAll()
        {
            return Ok(UserRepository.Users);
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetById(int id)
        {
            var user = UserRepository.Users.FirstOrDefault(u => u.Id == id);
            return user is not null ? Ok(user) : NotFound();
        }
    }
}
