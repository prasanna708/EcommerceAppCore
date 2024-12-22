using EcommerceAppCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAppCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly EcommerceDbContext _context;
        public ProductsController(EcommerceDbContext context)
        {
            _context = context;
        }

        //Getting All Products that are active
        [HttpGet]
        public IEnumerable<Product> Get(bool status)
        {
            try
            {
                if (status != null)
                    return _context.Products.Where(s => s.IsActive == status);
                else
                    return _context.Products.ToList();
            }
            catch (Exception ex)
            {
                //ErrorLogDetails(ex, "GET");
                throw ex;
            }
        }

        //Getting Product by ID that are active
        [HttpGet("{id}")]
        public ActionResult<Product> Get(int id, bool status)
        {
            try
            {
                Product product = new Product();
                if (status == null)
                {
                    product = _context.Products.Find(id);
                }
                else
                {
                    product = _context.Products.Where(p => p.IsActive == status && p.ProductId == id).FirstOrDefault();
                }
                if (product == null)
                    return NotFound();
                return Ok(product);
            }
            catch (Exception ex)
            {
                //ErrorLogDetails(ex, "GET by ID");
                throw ex;
            }
        }

        //Adding new Product
        [HttpPost]
        public IActionResult Post([FromBody] Product product)
        {
            try
            {
                if (product == null)
                    return BadRequest();
                _context.Products.Add(product);
                _context.SaveChanges();
                return CreatedAtAction(nameof(Get), new { id = product.ProductId }, product);
            }
            catch (Exception ex)
            {
                //ErrorLogDetails(ex, "POST");
                throw ex;
            }
        }

        //Updating existing Product
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Product Updatedproduct)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product == null)
                    return NotFound();
                product.Name = Updatedproduct.Name;
                product.Price = Updatedproduct.Price;
                product.Photo = Updatedproduct.Photo;
                product.Quantity = Updatedproduct.Quantity;
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                //ErrorLogDetails(ex, "PUT");
                throw ex;
            }
        }

        //Deleting an existing product by changing status to false
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product == null)
                    return NotFound();
                product.IsActive = false;
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                //ErrorLogDetails(ex, "DELETE");
                throw ex;
            }
        }
    }
}
