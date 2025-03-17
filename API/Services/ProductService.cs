using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.RequestHelpers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class ProductService : IProductService
{
    private readonly StoreContext _context;
    private readonly IMapper _mapper;
    private readonly ImageService _imageService;

    public ProductService(StoreContext context, IMapper mapper, ImageService imageService)
    {
        _context = context;
        _mapper = mapper;
        _imageService = imageService;
    }

    public async Task<PagedList<Product>> GetProducts(ProductParams productParams)
    {
        var query = _context.Products
            .Sort(productParams.OrderBy)
            .Search(productParams.SearchTerm)
            .Filter(productParams.Brands, productParams.Types)
            .AsQueryable();

        return await PagedList<Product>.ToPagedList(query,
            productParams.PageNumber, productParams.PageSize);
    }

    public async Task<Product?> GetProduct(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<(bool success, Product? product, string? error)> CreateProduct(CreateProductDto productDto)
    {
        var product = _mapper.Map<Product>(productDto);

        if (productDto.File != null)
        {
            var imageResult = await _imageService.AddImageAsync(productDto.File);
            if (imageResult.Error != null)
                return (false, null, imageResult.Error.Message);

            product.PictureUrl = imageResult.SecureUrl.AbsoluteUri;
            product.PublicId = imageResult.PublicId;
        }

        _context.Products.Add(product);
        var result = await _context.SaveChangesAsync() > 0;

        return result ? (true, product, null) : (false, null, "Problem creating product");
    }

    public async Task<(bool success, string? error)> UpdateProduct(UpdateProductDto updateProductDto)
    {
        var product = await _context.Products.FindAsync(updateProductDto.Id);
        if (product == null) return (false, "Product not found");

        _mapper.Map(updateProductDto, product);

        if (updateProductDto.File != null)
        {
            var imageResult = await _imageService.AddImageAsync(updateProductDto.File);
            if (imageResult.Error != null)
                return (false, imageResult.Error.Message);

            if (!string.IsNullOrEmpty(product.PublicId))
                await _imageService.DeleteImageAsync(product.PublicId);

            product.PictureUrl = imageResult.SecureUrl.AbsoluteUri;
            product.PublicId = imageResult.PublicId;
        }

        var result = await _context.SaveChangesAsync() > 0;
        return result ? (true, null) : (false, "Problem updating product");
    }

    public async Task<(bool success, string? error)> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return (false, "Product not found");

        if (!string.IsNullOrEmpty(product.PublicId))
            await _imageService.DeleteImageAsync(product.PublicId);

        _context.Products.Remove(product);
        var result = await _context.SaveChangesAsync() > 0;

        return result ? (true, null) : (false, "Problem deleting product");
    }

    public async Task<object> GetFilters()
    {
        var brands = await _context.Products.Select(x => x.Brand).Distinct().ToListAsync();
        var types = await _context.Products.Select(x => x.Type).Distinct().ToListAsync();
        return new { brands, types };
    }
} 