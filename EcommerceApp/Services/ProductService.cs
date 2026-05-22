using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Product> GetAll() => _context.Products.ToList();

    public List<Product> GetByCategory(string category) =>
        _context.Products.Where(p => p.Category == category).ToList();

    public List<Product> Search(string searchTerm) =>
        _context.Products
            .Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()))
            .ToList();

    public Product? GetById(int id) => _context.Products.Find(id);

    public Product Create(Product product)
    {
        if (product.Price < 0) throw new InvalidOperationException("Price cannot be negative.");
        if (product.StockQuantity < 0) throw new InvalidOperationException("Stock cannot be negative.");
        _context.Products.Add(product);
        _context.SaveChanges();
        return product;
    }

    public Product Update(int id, Product product)
    {
        var existing = _context.Products.Find(id) ?? throw new KeyNotFoundException($"Product {id} not found.");
        if (product.Price < 0) throw new InvalidOperationException("Price cannot be negative.");
        if (product.StockQuantity < 0) throw new InvalidOperationException("Stock cannot be negative.");

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.StockQuantity = product.StockQuantity;
        existing.Category = product.Category;

        _context.SaveChanges();
        return existing;
    }

    public void Delete(int id)
    {
        var product = _context.Products.Find(id) ?? throw new KeyNotFoundException($"Product {id} not found.");
        _context.Products.Remove(product);
        _context.SaveChanges();
    }
}
