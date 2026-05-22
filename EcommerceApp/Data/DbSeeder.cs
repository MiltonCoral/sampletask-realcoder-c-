using EcommerceApp.Models;

namespace EcommerceApp.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        if (context.Products.Any()) return;

        var products = new List<Product>
        {
            new() { Name = "Wireless Bluetooth Headphones", Description = "Over-ear noise cancelling headphones with 30hr battery", Price = 79.99m, StockQuantity = 25, Category = "Electronics" },
            new() { Name = "USB-C Charging Cable", Description = "Braided 6ft fast charging cable", Price = 12.99m, StockQuantity = 100, Category = "Electronics" },
            new() { Name = "Mechanical Keyboard", Description = "RGB backlit mechanical keyboard with blue switches", Price = 59.99m, StockQuantity = 15, Category = "Electronics" },
            new() { Name = "Cotton T-Shirt", Description = "Premium fitted crew neck tee", Price = 19.99m, StockQuantity = 50, Category = "Clothing" },
            new() { Name = "Denim Jeans", Description = "Slim fit stretch denim jeans", Price = 49.99m, StockQuantity = 30, Category = "Clothing" },
            new() { Name = "Running Sneakers", Description = "Lightweight breathable running shoes", Price = 89.99m, StockQuantity = 20, Category = "Clothing" },
            new() { Name = "Ceramic Coffee Mug", Description = "Handcrafted 12oz ceramic mug", Price = 14.99m, StockQuantity = 40, Category = "Home" },
            new() { Name = "Throw Pillow Set", Description = "Decorative square cushion covers, set of 2", Price = 24.99m, StockQuantity = 35, Category = "Home" },
            new() { Name = "LED Desk Lamp", Description = "Dimmable touch control desk lamp", Price = 34.99m, StockQuantity = 18, Category = "Home" },
            new() { Name = "Stainless Steel Water Bottle", Description = "Insulated 32oz sports bottle", Price = 22.99m, StockQuantity = 60, Category = "Home" },
            new() { Name = "Wireless Mouse", Description = "Ergonomic 2.4GHz wireless mouse", Price = 29.99m, StockQuantity = 0, Category = "Electronics" },
            new() { Name = "Canvas Tote Bag", Description = "Reusable shopping tote with reinforced handles", Price = 15.99m, StockQuantity = 45, Category = "Clothing" }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}
