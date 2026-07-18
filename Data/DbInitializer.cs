using AHUWeb.Models;

namespace AHUWeb.Data
{
    public static class DbInitializer
    {
        // Default login: admin / Admin@123  (change this after first run)
        // Gọi SAU khi db.Database.Migrate() đã chạy (xem Program.cs) — không dùng EnsureCreated()
        // ở đây vì nó xung đột với việc dùng Migrations (2 cơ chế tạo schema khác nhau).
        public static void Seed(ApplicationDbContext db)
        {
            if (!db.Users.Any(u => u.Username == "admin"))
            {
                db.Users.Add(new User
                {
                    Username = "admin",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "admin",
                    FullName = "Quản trị viên AHU"
                });
                db.SaveChanges();
            }
        }
    }
}
