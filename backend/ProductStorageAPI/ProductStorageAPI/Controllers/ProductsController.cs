using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductStorageAPI.Abstractions;
using ProductStorageAPI.DTOs.Product;

namespace ProductStorageAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> GetProducts([FromQuery] string? search, CancellationToken cancellationToken)
        {
            var products = await _productService.GetProductsAsync(search, cancellationToken);
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto?>> GetProductById(int id, CancellationToken cancellationToken)
        {
            var product = await _productService.GetProductByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto request, CancellationToken cancellationToken)
        {
            var product = await _productService.CreateProductAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto request, CancellationToken cancellationToken)
        {
            var product = await _productService.UpdateProductAsync(id, request, cancellationToken);
            if (!product)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            var product = await _productService.DeleteProductAsync(id, cancellationToken);
            if (!product)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
