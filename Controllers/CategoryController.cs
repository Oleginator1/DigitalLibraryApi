using CsvHelper;
using DigitalLibraryApi.Models;
using DigitalLibraryApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

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

        [HttpGet("export")]
        public IActionResult ExportCategories([FromQuery] string? name)
        {
            // 1️⃣ Filter by name if provided
            var filteredCategories = CategoryRepository.Categories
                .Where(c => string.IsNullOrEmpty(name) ||
                            c.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // 2️⃣ Write filtered data to CSV
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(filteredCategories);
            writer.Flush();
            memoryStream.Position = 0;

            // 3️⃣ Dynamic filename
            var fileName = string.IsNullOrEmpty(name)
                ? $"categories_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                : $"categories_export_{name}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            // 4️⃣ Return downloadable file
            return File(memoryStream.ToArray(), "text/csv", fileName);
        }
    }
}
