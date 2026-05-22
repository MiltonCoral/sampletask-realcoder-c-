using EcommerceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public ActionResult<OrderConfirmation> PlaceOrder(OrderRequest request)
    {
        try
        {
            var confirmation = _orderService.PlaceOrder(request);
            return StatusCode(201, confirmation);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("Insufficient stock"))
            {
                return Conflict(new { error = ex.Message });
            }
            return BadRequest(new { error = ex.Message });
        }
    }
}
