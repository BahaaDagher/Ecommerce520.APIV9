namespace Ecommerce520.APIV9.DTOs.Response
{
    public class ProductRelatedProductsResponse
    {
        public Product Product { get; set; } = new Product();
        public IEnumerable<Product> RelatedProducts { get; set; } = new List<Product>();
    }
}
