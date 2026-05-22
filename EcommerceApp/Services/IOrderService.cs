namespace EcommerceApp.Services;

public class OrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public List<CartItemDto> Items { get; set; } = new();
}

public class OrderConfirmation
{
    public int OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
}

public interface IOrderService
{
    OrderConfirmation PlaceOrder(OrderRequest request);
}
