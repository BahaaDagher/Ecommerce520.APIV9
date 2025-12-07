using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ecommerce520.APIV9.Areas.Customerr
{
    [Area("Customerr")]
    [Route("[area]/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public HomeController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpPost]
        public async Task<IActionResult> GetAll(FilterProductRequest filter, int page = 1)
        {
            //var products = _context.Products.Include(c => c.Category).AsQueryable();
            var products = await _productRepository.GetAsync(includes: [p=> p.ProductSubImages , p => p.ProductColors]); 

            // add filter 
            if (filter.Name != null)
            {
                products = products.Where(p => p.Name.Contains(filter.Name));
            }
            if (filter.MinPrice != null)
            {
                products = products.Where(p => p.Price - p.Price * (p.Discount / 100) >= filter.MinPrice);
            }
            if (filter.MaxPrice != null)
            {
                products = products.Where(p => p.Price - p.Price * (p.Discount / 100) <= filter.MaxPrice);
            }
            if (filter.CategoryId != null)
            {
                products = products.Where(p => p.CategoryId == filter.CategoryId);
            }
            if (filter.BrandId != null)
            {
                products = products.Where(p => p.BrandId == filter.BrandId);
            }
            if (filter.IsHot == true)
            {
                products = products.Where(p => p.Discount >= 50);
            }

            products = products.Skip((page - 1) * 8).Take(8);



            //ProductResponse productResponse = new ProductResponse();
            var productResponse = products.Adapt<IEnumerable<ProductResponse>>(); 

            return Ok(productResponse);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Item(int id)
        {
            //var product = _context.Products.Include(p=>p.Category).FirstOrDefault(p => p.Id == id);
            var product = await _productRepository.GetOneAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound( new ErrorModelResponse
                {
                    ErrorCode = 404  , 
                    ErrorMessage = "product not found"
                });
            }
            //var relatedProducts = _context.Products.Where(p=>p.Name.Contains(product.Name) && p.Id != id); 
            var relatedProducts = await _productRepository.GetAsync(p => p.CategoryId == product.CategoryId && p.Id != product.Id);
            relatedProducts = relatedProducts.Take(4);
            ProductRelatedProductsResponse productRelatedProductsVM = new ProductRelatedProductsResponse()
            {
                Product = product,
                RelatedProducts = relatedProducts
            };
            return Ok(productRelatedProductsVM);
        }
    }
}
