using Microsoft.AspNetCore.Mvc;
using Shop.Api.Entities;
using Shop.Api.Repositories;
using AutoMapper;
using Shop.Api.MiddleWare;
using Shop.Api.Enums;
using Stripe;
using Stripe.Checkout;
using Shop.Api.Payments;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/v1/order")]
public class OrderController : ControllerBase
{
    private readonly IOrderRepository repository;
    private readonly IShopItemRepository repository2;
    private readonly IPaymentProcessor paymentProcessor;
    private readonly ILogger<OrderController> logger;
    private IMapper mapper;
    private readonly IConfiguration configuration;

    public OrderController(ILogger<OrderController> logger, IMapper mapper, IOrderRepository repository, 
        IShopItemRepository repository2, IConfiguration configuration, IPaymentProcessor paymentProcessor)
    {
        this.configuration = configuration;
        this.logger = logger;
        this.mapper = mapper;
        this.repository = repository;
        this.repository2 = repository2;
        this.paymentProcessor = paymentProcessor;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderModeWithItemsDto>>> GetOrdersAsync([FromQuery] Boolean CompletedOrders = false)
    {       
        try{
            UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
            
            var orders = (await repository.GetOrdersAsync(user.Id, CompletedOrders)).Select(order => 
                {
                    return this.mapper.Map<OrderModeWithItemsDto>(order);
                }
            );

            return Ok(new{orders = orders.ToList()});
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<ActionResult<IEnumerable<ReviewModel>>> UpdateOrderAsync(Guid id)
    {       
        try{
            UserModel user = (UserModel)Request.HttpContext.Items["User"]!;

            var isUserAuthorizedToCompleteOrder = user.Role == RoleEnum.Worker || user.Role == RoleEnum.Admin;
            if(!isUserAuthorizedToCompleteOrder) return StatusCode(403);
            
            OrderDetails orderDetails = new OrderDetails() {
                CompleteDate = DateTimeOffset.UtcNow,
                WorkerConfirmingDeliveryId = user.Id,
                IsOrderCompleted = true,
            };

            var orderUpdatedCount = await repository.CompleteOrder(id, orderDetails);

            if(orderUpdatedCount>0) return NoContent();
            
            return NotFound(new {error = "Order not found"});
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<string>> CreateOrderAsync([FromBody] CreateOrderDto createOrder)
    {       
        try{


            UserModel user = (UserModel)Request.HttpContext.Items["User"]!;

            List<SessionLineItemOptions> items = new List<SessionLineItemOptions>();
            await Task.WhenAll(createOrder.OrderedItems.Select(
                async orderedItem => {
                    ShopItem item = await repository2.GetShopItemAsync(orderedItem.ShopItemId);
                    if(item == null) return;

                    SessionLineItemOptions stripeItem = new SessionLineItemOptions();
                    
                    stripeItem.PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)item.Price * 100,
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Name,
                        },

                    };
                    stripeItem.Quantity = (long)orderedItem.Amount;
                    items.Add(stripeItem);
                }
            ));

            if(items.Count == 0) return StatusCode(400, new {error = "No items to add to order found in request "});

            var result = await this.paymentProcessor.ProcessPayment(items, createOrder.RedirectUrlSuccess, createOrder.RedirectUrlFailure);

            OrderModel order = new(){
                Id = Guid.NewGuid(),
                OrderedItems = createOrder.OrderedItems,
                Address = createOrder.Address,
                City = createOrder.City,
                Region = createOrder.Region,
                PostalCode = createOrder.PostalCode,
                Country = createOrder.Country,
                ExpectedDeliveryTime = DateTimeOffset.UtcNow.AddDays(2),
                UserId = user.Id,
                CreatedDate = DateTimeOffset.UtcNow,
                TransactionId = result.Id,
                TransactionConfirmed = false,
                Details = new OrderDetails(){
                    CompleteDate = null,
                    WorkerConfirmingDeliveryId = null,
                    IsOrderCompleted = false
                }
            };

            await repository.CreateOrderAsync(order);

            return Ok(new{ url = result.Url});    
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Index()
    {
      try
      {
        var webHookKey = this.configuration.GetSection("Stripe:webHook").Value;
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        var stripeEvent = EventUtility.ConstructEvent(
          json,
          Request.Headers["Stripe-Signature"],
          webHookKey
        );

       if (stripeEvent.Type == Events.CheckoutSessionCompleted)
        {
          var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

          await repository.ConfirmPaymentAsync(session.Id);
        }
        // ... handle other event types
        else
        {
            Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
        }

        return Ok();
      }
      catch (StripeException e)
      {
        return BadRequest();
      }
    }

    

}