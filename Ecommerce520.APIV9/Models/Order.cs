namespace Ecommerce520.APIV9.Models
{
    public enum OrderStatus
    {
        Pending ,
        InProgress, 
        Shipped , 
        Completed , 
        Canceled ,
    }
    public enum TransactionType
    {
        Card ,
        Cash
    }
    public class Order
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; } 
        public TransactionType TransactionType { get; set; }  = TransactionType.Card;
        public string? SessionId { get; set; }
        public string? TransactionId { get; set; }
    }
}
