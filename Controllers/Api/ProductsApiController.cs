using AHUWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Controllers.Api
{
    // Lab 17: Web API don gian de ReactJS (Views/Home/ReactDemo.cshtml) goi qua fetch().
    // Chi doc du lieu, khong co side effect - an toan tuyet doi voi du lieu dang chay that.
    [ApiController]
    [Route("api/products")]
    public class ProductsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProductsApiController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /api/products/latest
        [HttpGet("latest")]
        public async Task<IActionResult> Latest()
        {
            var products = await _db.Products
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .Select(p => new { p.Id, p.Name, p.Price, p.Image })
                .ToListAsync();

            return Ok(products);
        }
    }
}
