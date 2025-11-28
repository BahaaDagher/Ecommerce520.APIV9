using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce520.APIV9.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[Area]/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE} ,{SD.ADMIN_ROLE} ,{SD.EMPLOYEE_ROLE} ")]
    public class BrandsController : ControllerBase
    {
        IRepository<Brand> _brandRepository; //= new Repository<Brand>();

        public BrandsController(IRepository<Brand> brandRepository)
        {
            _brandRepository = brandRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            //var brands = _context.Brands.AsQueryable();
            var brands = await _brandRepository.GetAsync();

            return Ok(brands.AsEnumerable());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            //var brand = _context.Brands.FirstOrDefault(c=>c.Id == id);
            var brand = await _brandRepository.GetOneAsync(c => c.Id == id);
            if (brand is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "Brand not found."
                });
            }
            //var updateBrandVM = new UpdateBrandVM()
            //{
            //    Id = brand.Id,
            //    Name = brand.Name,
            //    Description = brand.Description,
            //    Status = brand.Status , 
            //    Img = brand.Img
            //};
            var updateBrandVM = brand.Adapt<UpdateBrandResponse>();

            return Ok(updateBrandVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBrandRequest CreateBrandRequest)
        {
            
            var brand = CreateBrandRequest.Adapt<Brand>();
            if (CreateBrandRequest.FormImg is not null)
            {
                if (CreateBrandRequest.FormImg.Length > 0)
                {
                    //var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                    var fileName = Guid.NewGuid().ToString() + "-" + CreateBrandRequest.FormImg.FileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\brand_images\\", fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        CreateBrandRequest.FormImg.CopyTo(stream);
                    }
                    brand.Img = fileName;
                }
            }
            await _brandRepository.AddAsync(brand);
            await _brandRepository.CommitAsync();
            return CreatedAtAction(nameof(GetOne), new { id = brand.Id }, brand);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateBrandRequest UpdateBrandVM)
        {
            var brandInDB = await _brandRepository.GetOneAsync(c => c.Id == id, trackd: false);

            if (brandInDB is null )
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "Brand not found."
                });
            }
             var brand = UpdateBrandVM.Adapt<Brand>();
            if (UpdateBrandVM.FormImg is not null)
            {
                if (UpdateBrandVM.FormImg.Length > 0)
                {
                    //var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                    var fileName = Guid.NewGuid().ToString() + "-" + UpdateBrandVM.FormImg.FileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\brand_images\\", fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        UpdateBrandVM.FormImg.CopyTo(stream);
                    }
                    brand.Img = fileName;
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\brand_images\\", brandInDB.Img);

                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }
            }
            else
            {
                brand.Img = brandInDB.Img;
            }
            //_context.Brands.Update(brand);
            //_context.SaveChanges();
            _brandRepository.Update(brand);
            await _brandRepository.CommitAsync();
            return Ok(new
            {
                msg = "Brand Updated Successfully"
            }); 
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            //var brand = _context.Brands.FirstOrDefault(c => c.Id == id);
            var brand = await _brandRepository.GetOneAsync(c => c.Id == id);
            if (brand is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "Brand not found."
                });
            }

            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\brand_images\\", brand.Img);

            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            //_context.Brands.Remove(brand);
            //_context.SaveChanges();
            _brandRepository.Delete(brand);
            await _brandRepository.CommitAsync();
            return NoContent();
        }
    }
}
