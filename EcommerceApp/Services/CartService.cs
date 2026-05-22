using EcommerceApp.Models;

namespace EcommerceApp.Services;

public class CartService : ICartService
{
    private readonly IProductService _productService;

    public CartService(IProductService productService)
    {
        _productService = productService;
    }

    public CartDto AddItem(List<CartItemDto> currentCart, int productId, int quantity)
    {
        var product = _productService.GetById(productId) ?? throw new InvalidOperationException("Product not found.");
        if (product.StockQuantity < quantity) throw new InvalidOperationException("Insufficient stock.");

        var existing = currentCart.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
        {
            var newQty = existing.Quantity + quantity;
            if (product.StockQuantity < newQty) throw new InvalidOperationException("Insufficient stock.");
            existing.Quantity = newQty;
        }
        else
        {
            currentCart.Add(new CartItemDto
            {
                ProductId = productId,
                ProductName = product.Name,
                Quantity = quantity,
                UnitPrice = product.Price
            });
        }

        return GetCart(currentCart);
    }

    public CartDto UpdateItem(List<CartItemDto> currentCart, int productId, int quantity)
    {
        var product = _productService.GetById(productId) ?? throw new InvalidOperationException("Product not found.");
        if (quantity <= 0) return RemoveItem(currentCart, productId);
        if (product.StockQuantity < quantity) throw new InvalidOperationException("Insufficient stock.");

        var item = currentCart.FirstOrDefault(i => i.ProductId == productId) ?? throw new InvalidOperationException("Item not in cart.");
        item.Quantity = quantity;
        return GetCart(currentCart);
    }

    public CartDto RemoveItem(List<CartItemDto> currentCart, int productId)
    {
        currentCart.RemoveAll(i => i.ProductId == productId);
        return GetCart(currentCart);
    }

    public CartDto GetCart(List<CartItemDto> currentCart) => new() { Items = currentCart };
}
