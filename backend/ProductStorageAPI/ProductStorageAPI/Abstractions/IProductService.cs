using ProductStorageAPI.DTOs.Product;

namespace ProductStorageAPI.Abstractions
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetProductsAsync(string? search, CancellationToken cancellationToken = default);
        Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ProductDto> CreateProductAsync(CreateProductDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateProductAsync(int id, UpdateProductDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    }
}