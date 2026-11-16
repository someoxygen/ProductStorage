using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductStorageAPI.Abstractions;
using ProductStorageAPI.Data;
using ProductStorageAPI.DTOs.Product;
using ProductStorageAPI.Models;

namespace ProductStorageAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto request, CancellationToken cancellationToken = default)
        {
            var name = request.Name.Trim();
            var exists = await _context.Products.AnyAsync(x => x.Name == name, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Product with this name already exists.");
            }

            var product = new Product
            {
                Name = name,
                Description = request.Description,
                Price = request.Price,
                Amount = request.Amount,
                IsActive = request.IsActive,
                CreateDate = DateTime.Now,
            };
            
            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Amount = product.Amount,
                IsActive = product.IsActive,
                CreateDate = product.CreateDate,
            };
        }

        public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if(product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Amount = x.Amount,
                    IsActive = x.IsActive,
                    CreateDate = x.CreateDate,
                    UpdateDate = x.UpdateDate
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<ProductDto>> GetProductsAsync(string? search, CancellationToken cancellationToken = default)
        {
            var query = _context.Products.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => EF.Functions.Like(x.Name, $"{search.Trim()}"));
            }
            
            return await query
                .OrderByDescending(x => x.CreateDate)
                .Select(x => new ProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Amount = x.Amount,
                    IsActive = x.IsActive,
                    CreateDate = x.CreateDate,
                    UpdateDate = x.UpdateDate
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateProductAsync(int id, UpdateProductDto request, CancellationToken cancellationToken = default)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if(product == null)
            {
                return false;
            }

            var existingProduct = await _context.Products.FirstOrDefaultAsync(x => x.Name == request.Name && x.Id != id, cancellationToken);
            if(existingProduct != null)
            {
                throw new InvalidOperationException("Product with this name already exists.");
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Amount = request.Amount;
            product.IsActive = request.IsActive;
            product.CreateDate = request.CreateDate;
            product.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
