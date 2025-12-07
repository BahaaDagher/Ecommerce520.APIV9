using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Diagnostics.Eventing.Reader;

namespace Ecommerce520.APIV9.Areas.Customerr
{
    [Area("Customerr")]
    [Route("[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CartsController : ControllerBase
    {
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Promotion> _PromotionRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;

        public CartsController(IRepository<Cart> cartRepository, UserManager<ApplicationUser> userManager, IProductRepository productRepository, IRepository<Promotion> promotionRepository, IRepository<Order> orderRepository)
        {
            _cartRepository = cartRepository;
            _userManager = userManager;
            _productRepository = productRepository;
            _PromotionRepository = promotionRepository;
            _orderRepository = orderRepository;
        }
        [HttpGet("")]
        public async Task<IActionResult> Index(string? code = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "user Not found",
                });
            }
            var cart = await _cartRepository.GetAsync(c => c.ApplicationUserId == user.Id, [p => p.Product]);
            if (cart is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "Cart Not found",
                });
            }
            var ErrorMessage = "";
            if (code != null)
            {
                var promotion = await _PromotionRepository.GetOneAsync(p => p.Code == code);
                if (promotion != null)
                {
                    if (promotion.IsValid && promotion.ExpiryDate > DateTime.UtcNow && promotion.MaxUsage > 0)
                    {
                        var productInCart = cart.FirstOrDefault(c => c.ProductId == promotion.productId);
                        if (productInCart is not null)
                        {
                            productInCart.price -= productInCart.price * (promotion.Discount / 100);
                            promotion.MaxUsage--;
                            await _cartRepository.CommitAsync();
                            //TempData["Success"] = $"Promotion Applied! You get {promotion.Discount}% off on {promotion.Product.Name}";
                        }
                        else
                        {
                            ErrorMessage = "This Promotion Code is not applicable to any product in your cart";
                        }
                    }
                    else
                    {
                        promotion.IsValid = false;
                        ErrorMessage = "Expired Promotion Code";
                    }
                }
                else
                {
                    ErrorMessage = "Invalid Promotion Code";
                }
            }
            var totalPrice = cart.Sum(c => c.price * c.Count);
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                return Ok(cart);

            }
            return BadRequest(new ErrorModelResponse
            {
                ErrorCode = 400,
                ErrorMessage = ErrorMessage,
            });

        }
        [HttpPost("")]
        public async Task<IActionResult> AddToCart(int productId, int count)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "user Not found",
                });
            }
            var product = await _productRepository.GetOneAsync(p => p.Id == productId);
            if (product is null)
            {
                return NotFound();
            }
            if (count > product.Quantity)
            {
                return BadRequest(new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = "this Greater that the quentity of the Product",
                });
            }
            var isExist = await _cartRepository.GetOneAsync(c => c.ProductId == productId && c.ApplicationUserId == user.Id);
            if (isExist != null)
            {
                isExist.Count += count;
                _cartRepository.Update(isExist);
            }
            else
            {
                Cart cart = new Cart()
                {
                    ProductId = productId,
                    Count = count,
                    ApplicationUserId = user.Id,
                    price = (product.Price - (product.Price * (product.Discount / 100)))
                };
                await _cartRepository.AddAsync(cart);
            }
            await _cartRepository.CommitAsync();
            return Ok(new
            {
                msg = "product added to cart successfully"
            });
        }
        [HttpPut("{productId}/Increment")]
        public async Task<IActionResult> Increment(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "user Not found",
                });
            }
            var cart = await _cartRepository.GetOneAsync(c => c.ProductId == productId && c.ApplicationUserId == user.Id);
            if (cart == null)
            {
                return NotFound();
            }
            var product = await _productRepository.GetOneAsync(p => p.Id == productId);
            if (product is null)
            {
                return NotFound();
            }
            if (cart.Count == product.Quantity)
            {
                return BadRequest(new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = "you cant order a quntity over the product quentity",
                });
            }
            cart.Count++;
            _cartRepository.Update(cart);
            await _cartRepository.CommitAsync();
            return Ok(new
            {
                msg = "Increment successfully"
            });
        }
        [HttpPut("{productId}/Decrement")]
        public async Task<IActionResult> Decrement(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "user Not found",
                });
            }
            var cart = await _cartRepository.GetOneAsync(c => c.ProductId == productId && c.ApplicationUserId == user.Id);
            if (cart == null)
            {
                return NotFound();
            }
            var product = await _productRepository.GetOneAsync(p => p.Id == productId);
            if (product is null)
            {
                return NotFound();
            }
            if (cart.Count == 1)
            {
                await DeleteProduct(productId);
                //return RedirectToAction(nameof(Index));
            }
            cart.Count--;
            _cartRepository.Update(cart);
            await _cartRepository.CommitAsync();
            return Ok(new
            {
                msg = "Decrement successfully"
            });
        }
        [HttpPut("{productId}/DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "user Not found",
                });
            }
            var cart = await _cartRepository.GetOneAsync(c => c.ProductId == productId && c.ApplicationUserId == user.Id);
            if (cart == null)
            {
                return NotFound();
            }
            _cartRepository.Delete(cart);
            await _cartRepository.CommitAsync();
            return Ok(new
            {
                msg = "Deleted successfully"
            });
        }
        [HttpGet("pay")]
        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "user Not found",
                });
            }
            var carts = await _cartRepository.GetAsync(c => c.ApplicationUserId == user.Id, [p => p.Product]);
            if (carts is null   || carts.Count()== 0 )
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "carts empty ",
                });
            }
            Order order = new Order()
            {
                ApplicationUserId = user.Id,
                TotalPrice = carts.Sum(c => (c.price * c.Count)),
                OrderStatus = OrderStatus.Pending, 
            };
            var AddedOrder  = await _orderRepository.AddAsync(order); 
            await _orderRepository.CommitAsync();
            var orderId = AddedOrder.Entity.Id; 
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customerr/Checkout/Success?orderId={orderId}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customerr/Checkout/Cancel?orderId={orderId}",
            };
            foreach (var item in carts)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                            Description = item.Product.Description,
                        },
                        UnitAmount = (long)item.price * 100,
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            var session = service.Create(options);

            AddedOrder.Entity.SessionId = session.Id;
            await _orderRepository.CommitAsync(); 
            return Ok(new
            {
                sessionUrl = session.Url
            }); 
        }
    }
}
