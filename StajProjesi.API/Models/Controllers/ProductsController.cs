using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajProjesi.API.Data;
using StajProjesi.API.Models;

namespace StajProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                return await _context.Products.ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ürünler getirilirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // GET: api/products/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                    return NotFound();

                return product;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ürün getirilirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = product.Id },
                    product
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ürün oluşturulurken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // PUT: api/products/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(
            int id,
            Product product)
        {
            try
            {
                if (id != product.Id)
                    return BadRequest();

                _context.Entry(product).State =
                    EntityState.Modified;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ürün güncellenirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/products/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product =
                    await _context.Products.FindAsync(id);

                if (product == null)
                    return NotFound();

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ürün silinirken bir hata oluştu.",
                    error = ex.Message
                });
            }
        }
    }
}