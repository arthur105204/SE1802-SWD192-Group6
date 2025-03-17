<<<<<<< HEAD
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.Services;

namespace API.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProducts([FromQuery] ProductParams productParams)
        {
            var products = await _productService.GetProducts(productParams);
            Response.AddPaginationHeader(products.Metadata);
            return products;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetProduct(id);
            if (product == null) return NotFound();
            return product;
        }

=======
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.RequestHelpers;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class ProductsController(StoreContext context, IMapper mapper, 
        ImageService imageService) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProducts(
            [FromQuery] ProductParams productParams)
        {
            var query = context.Products
                .Sort(productParams.OrderBy)
                .Search(productParams.SearchTerm)
                .Filter(productParams.Brands, productParams.Types)
                .AsQueryable();

            var products = await PagedList<Product>.ToPagedList(query,
                productParams.PageNumber, productParams.PageSize);

            Response.AddPaginationHeader(products.Metadata);

            return products;
        }

        [HttpGet("{id}")] // api/products/2
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await context.Products.FindAsync(id);

            if (product == null) return NotFound();

            return product;
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var brands = await context.Products.Select(x => x.Brand).Distinct().ToListAsync();
            var types = await context.Products.Select(x => x.Type).Distinct().ToListAsync();

            return Ok(new { brands, types });
        }

>>>>>>> e407d1a1383d87c5d2424c45fca6b1bb815b00ed
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(CreateProductDto productDto)
        {
<<<<<<< HEAD
            var (success, product, error) = await _productService.CreateProduct(productDto);
            if (!success) return BadRequest(new ProblemDetails { Title = error });
            return CreatedAtAction(nameof(GetProduct), new { Id = product!.Id }, product);
=======
            var product = mapper.Map<Product>(productDto);

            if (productDto.File != null)
            {
                var imageResult = await imageService.AddImageAsync(productDto.File);

                if (imageResult.Error != null)
                {
                    return BadRequest(imageResult.Error.Message);
                }

                product.PictureUrl = imageResult.SecureUrl.AbsoluteUri;
                product.PublicId = imageResult.PublicId;
            }

            context.Products.Add(product);

            var result = await context.SaveChangesAsync() > 0;

            if (result) return CreatedAtAction(nameof(GetProduct), new { Id = product.Id }, product);

            return BadRequest("Problem creating new procuct");
>>>>>>> e407d1a1383d87c5d2424c45fca6b1bb815b00ed
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<ActionResult> UpdateProduct(UpdateProductDto updateProductDto)
        {
<<<<<<< HEAD
            var (success, error) = await _productService.UpdateProduct(updateProductDto);
            if (!success) return BadRequest(new ProblemDetails { Title = error });
            return NoContent();
=======
            var product = await context.Products.FindAsync(updateProductDto.Id);

            if (product == null) return NotFound();

            mapper.Map(updateProductDto, product);

            if (updateProductDto.File != null) 
            {
                var imageResult = await imageService.AddImageAsync(updateProductDto.File);

                if (imageResult.Error != null)
                    return BadRequest(imageResult.Error.Message);

                if (!string.IsNullOrEmpty(product.PublicId))
                    await imageService.DeleteImageAsync(product.PublicId);

                product.PictureUrl = imageResult.SecureUrl.AbsoluteUri;
                product.PublicId = imageResult.PublicId;
            }

            var result = await context.SaveChangesAsync() > 0;

            if (result) return NoContent();

            return BadRequest("Problem updating product");
>>>>>>> e407d1a1383d87c5d2424c45fca6b1bb815b00ed
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
<<<<<<< HEAD
            var (success, error) = await _productService.DeleteProduct(id);
            if (!success) return BadRequest(new ProblemDetails { Title = error });
            return Ok();
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            return Ok(await _productService.GetFilters());
        }
    }
} 
=======
            var product = await context.Products.FindAsync(id);

            if (product == null) return NotFound();

            if (!string.IsNullOrEmpty(product.PublicId))
                    await imageService.DeleteImageAsync(product.PublicId);

            context.Products.Remove(product);

            var result = await context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Problem deleting the product");
        }
    }
}
>>>>>>> e407d1a1383d87c5d2424c45fca6b1bb815b00ed
