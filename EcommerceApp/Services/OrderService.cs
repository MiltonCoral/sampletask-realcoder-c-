using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public OrderConfirmation PlaceOrder(OrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName) ||
            string.IsNullOrWhiteSpace(request.CustomerEmail) ||
            string.IsNullOrWhiteSpace(request.ShippingAddress))
        {
            throw new InvalidOperationException("All customer fields are required.");
        }
        if (!request.CustomerEmail.Contains('@'))
        {
            throw new InvalidOperationException("Invalid email format.");
        }
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty.");
        }

        foreach (var item in request.Items)
        {
            var product = _context.Products.Find(item.ProductId);
            if (product == null) throw new InvalidOperationException($"Product {item.ProductId} not found.");
            if (product.StockQuantity < item.Quantity) throw new InvalidOperationException($"Insufficient stock for {product.Name}.");
        }

        var order = new Order
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            ShippingAddress = request.ShippingAddress,
            TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice) * 1.08m,
            OrderDate = DateTime.UtcNow,
            OrderItems = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        _context.Orders.Add(order);

        foreach (var item in request.Items)
        {
            var product = _context.Products.Find(item.ProductId)!;
            product.StockQuantity -= item.Quantity;
        }

        _context.SaveChanges();

        return new OrderConfirmation
        {
            OrderId = order.Id,
            CustomerEmail = order.CustomerEmail,
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate
        };
    }
}
