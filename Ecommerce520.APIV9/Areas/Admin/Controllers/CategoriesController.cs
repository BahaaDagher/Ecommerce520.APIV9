using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce520.APIV9.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[Area]/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE} ,{SD.ADMIN_ROLE} ,{SD.EMPLOYEE_ROLE} ")]
    public class CategoriesController : ControllerBase
    {
        IRepository<Category> _categoryRepository; // = new Repository<Category>(); 

        public CategoriesController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            //var categories = _context.Categories.AsQueryable();
            var categories = await _categoryRepository.GetAsync(cancellationToken: cancellationToken);

            return Ok(categories.AsEnumerable());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id, CancellationToken cancellationToken)
        {
            //var category = _context.Categories.FirstOrDefault(c=>c.Id == id);
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id, cancellationToken: cancellationToken);
            if (category is null)
            {
                return NotFound( new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "Category not found."
                }); 
            }
            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category, CancellationToken cancellationToken)
        {
            //_context.Categories.Add(category);
            //_context.SaveChanges();
            await _categoryRepository.AddAsync(category, cancellationToken);
            await _categoryRepository.CommitAsync(cancellationToken);
            return CreatedAtAction(nameof(GetOne), new { id = category.Id }, category);
        }
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE} ,{SD.ADMIN_ROLE} ")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id , Category category, CancellationToken cancellationToken)
        {
            var categoryInDb = await _categoryRepository.GetOneAsync(c => c.Id == id, trackd:false, cancellationToken: cancellationToken);
            if (categoryInDb is null)
            {
                return NotFound( new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "Category not found."
                });
            }
            //_context.Categories.Update(category);
            //_context.SaveChanges();
            category.Id = id;
            _categoryRepository.Update(category);
            await _categoryRepository.CommitAsync(cancellationToken: cancellationToken);
            return Ok(new
            {
                Message = "Category updated successfully."
            });
        }
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE} ,{SD.ADMIN_ROLE} ")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            //var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id, cancellationToken: cancellationToken);
            if (category is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "Category not found."
                });
            }

            //_context.Categories.Remove(category);
            //_context.SaveChanges();
            _categoryRepository.Delete(category);
            await _categoryRepository.CommitAsync(cancellationToken);
            return NoContent();
        }
    }
}
