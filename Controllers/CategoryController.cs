using Microsoft.AspNetCore.Mvc;
using DigitalLibraryApi.Models;
using DigitalLibraryApi.Repositories;

namespace DigitalLibraryApi.Controllers
{
    [ApiController]
    [Route("categories")]
    public class CategoriesController : ControllerBase
    {
        // GET /categories
        [HttpGet]
        public ActionResult<IEnumerable<Category>> GetAllCategories()
        {
            return Ok(CategoryRepository.Categories);
        }

        // POST /categories
        [HttpPost]
        public ActionResult<Category> AddCategory([FromBody] Category newCategory)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            newCategory.Id = CategoryRepository.Categories.Max(c => c.Id) + 1;
            CategoryRepository.Categories.Add(newCategory);

            return CreatedAtAction(nameof(GetAllCategories), new { id = newCategory.Id }, newCategory);
        }
    }
}
