namespace Ecommerce520.APIV9.Models
{
    public class Cart
    {
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser? ApplicationUser { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Count { get; set; }
        public decimal price { get; set; }
    }
}
