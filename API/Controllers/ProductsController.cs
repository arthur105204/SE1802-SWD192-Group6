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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(CreateProductDto productDto)
        {
            var (success, product, error) = await _productService.CreateProduct(productDto);
            if (!success) return BadRequest(new ProblemDetails { Title = error });
            return CreatedAtAction(nameof(GetProduct), new { Id = product!.Id }, product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<ActionResult> UpdateProduct(UpdateProductDto updateProductDto)
        {
            var (success, error) = await _productService.UpdateProduct(updateProductDto);
            if (!success) return BadRequest(new ProblemDetails { Title = error });
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
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