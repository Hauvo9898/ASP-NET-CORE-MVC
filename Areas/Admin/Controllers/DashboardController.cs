using AHUWeb.Data;
using AHUWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class DashboardController : AdminBaseController
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Admin/Dashboard — thay cho renderAdminOverview() (tab "overview")
        public async Task<IActionResult> Index()
        {
            var orders = await _db.Orders.Include(o => o.OrderDetails).ToListAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalRevenue = orders.Where(o => o.Status != "cancelled").Sum(o => o.Total),
                TotalOrders = orders.Count,
                TotalUsers = await _db.Users.CountAsync(u => u.Role == "customer"),
                TotalProductsSold = orders.Where(o => o.Status != "cancelled")
                    .SelectMany(o => o.OrderDetails).Sum(od => od.Quantity),
                RecentOrders = orders.OrderByDescending(o => o.Date).Take(5).ToList(),
                OrdersByStatus = orders.GroupBy(o => o.Status).ToDictionary(g => g.Key, g => g.Count())
            };

            return View(vm);
        }

        // GET /Admin/Dashboard/Reports — thay cho renderAdminReports() (tab "reports")
        public async Task<IActionResult> Reports()
        {
            var orders = await _db.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.Status != "cancelled")
                .ToListAsync();

            var since = DateTime.Today.AddDays(-13);
            var byDay = orders
                .Where(o => o.Date.Date >= since)
                .GroupBy(o => o.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.Total));

            var vm = new AdminReportsViewModel
            {
                TotalRevenue = orders.Sum(o => o.Total),
                TotalProductsSold = orders.SelectMany(o => o.OrderDetails).Sum(od => od.Quantity),
                TotalUsers = await _db.Users.CountAsync(u => u.Role == "customer"),
                RevenueByDay = Enumerable.Range(0, 14)
                    .Select(i => since.AddDays(i))
                    .Select(d => (d.ToString("dd/MM"), byDay.TryGetValue(d, out var r) ? r : 0m))
                    .ToList()
            };

            return View(vm);
        }
    }
}
