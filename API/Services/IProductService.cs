using API.DTOs;
using API.Entities;
using API.RequestHelpers;

namespace API.Services;

public interface IProductService
{
    Task<PagedList<Product>> GetProducts(ProductParams productParams);
    Task<Product?> GetProduct(int id);
    Task<(bool success, Product? product, string? error)> CreateProduct(CreateProductDto productDto);
    Task<(bool success, string? error)> UpdateProduct(UpdateProductDto updateProductDto);
    Task<(bool success, string? error)> DeleteProduct(int id);
    Task<object> GetFilters();
} 