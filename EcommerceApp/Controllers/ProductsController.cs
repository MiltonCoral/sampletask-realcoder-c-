using EcommerceApp.Models;
using EcommerceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public ActionResult<List<Product>> GetAll([FromQuery] string? category, [FromQuery] string? search)
    {
        if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(search))
        {
            return _productService.Search(search).Where(p => p.Category == category).ToList();
        }
        if (!string.IsNullOrEmpty(category))
        {
            return _productService.GetByCategory(category);
        }
        if (!string.IsNullOrEmpty(search))
        {
            return _productService.Search(search);
        }
        return _productService.GetAll();
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = _productService.GetById(id);
        if (product == null) return NotFound();
        return product;
    }

    [HttpPost]
    public ActionResult<Product> Create(Product product)
    {
        try
        {
            var created = _productService.Create(product);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public ActionResult<Product> Update(int id, Product product)
    {
        try
        {
            return _productService.Update(id, product);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        try
        {
            _productService.Delete(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
