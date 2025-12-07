using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Threading.Tasks;

namespace Ecommerce520.APIV9.Areas.Customerr
{
    [Area("Customerr")]
    [Route("[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CheckoutController : ControllerBase
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IEmailSender _emailSender;

        public CheckoutController(IRepository<Order> orderRepository, IEmailSender emailSender, IRepository<Cart> cartRepository, IRepository<OrderItem> orderItemRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _emailSender = emailSender;
            _cartRepository = cartRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
        }

        [HttpPost("Success")]
        public async Task<IActionResult> Success(int orderId )
        {
            var order = await _orderRepository.GetOneAsync(o => o.Id == orderId , includes: [o=>o.ApplicationUser]); 
            if (order is null)
            {
                return BadRequest(new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = "order Not found",
                });
            }
            // send mail 
            var user  = order.ApplicationUser;  
            await _emailSender.SendEmailAsync(user.Email, " Paymanent",$"<h1> your payment is in Progress</a>  </h1>");

            //  change  status to inprogress 
            order.OrderStatus = OrderStatus.InProgress;
            var service = new SessionService();
            var session = service.Get(order.SessionId);
            order.TransactionId = session.PaymentIntentId; 
            await _orderRepository.CommitAsync();
            // add order items from Cart and delete it from Cart 
            var carts = await _cartRepository.GetAsync(c=>c.ApplicationUserId == user.Id); 
            foreach(var item in carts )
            {
                OrderItem orderItem = new OrderItem()
                {
                    ProductId  = item.ProductId,
                    OrderId  = order.Id , 
                    count = item.Count,
                    Price  = item.price , 
                };
                await _orderItemRepository.AddAsync(orderItem);
                var product = await _productRepository.GetOneAsync(p=>p.Id == item.ProductId);

                // decrease the quentity of the products 
                product.Quantity -= item.Count; 
                await _productRepository.CommitAsync();
                _cartRepository.Delete(item); 
            }
            await _orderItemRepository.CommitAsync();
            

            return Ok( new
            {
                msg  = "paid Successfully "
            }); 
        }
        [HttpPost("Cancel")]
        public IActionResult Cancel()
        {
            return Ok(new
            {
                msg = "canceled Successfully "
            });
        }
    }
}
