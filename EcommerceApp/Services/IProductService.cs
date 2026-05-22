using EcommerceApp.Models;

namespace EcommerceApp.Services;

public interface IProductService
{
    List<Product> GetAll();
    List<Product> GetByCategory(string category);
    List<Product> Search(string searchTerm);
    Product? GetById(int id);
    Product Create(Product product);
    Product Update(int id, Product product);
    void Delete(int id);
}
