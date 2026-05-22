namespace EcommerceApp.Services;

public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Subtotal => Items.Sum(i => i.TotalPrice);
    public decimal Tax => Subtotal * 0.08m;
    public decimal GrandTotal => Subtotal + Tax;
}

public interface ICartService
{
    CartDto AddItem(List<CartItemDto> currentCart, int productId, int quantity);
    CartDto UpdateItem(List<CartItemDto> currentCart, int productId, int quantity);
    CartDto RemoveItem(List<CartItemDto> currentCart, int productId);
    CartDto GetCart(List<CartItemDto> currentCart);
}
