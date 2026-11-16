using ProductStorageAPI.DTOs.Product;

namespace ProductStorageAPI.Abstractions
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetProductsAsync(int userId, string? search, CancellationToken cancellationToken = default);
        Task<ProductDto?> GetProductByIdAsync(int userId, int id, CancellationToken cancellationToken = default);
        Task<ProductDto> CreateProductAsync(int userId, CreateProductDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateProductAsync(int userId, int id, UpdateProductDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteProductAsync(int userId, int id, CancellationToken cancellationToken = default);
    }
}