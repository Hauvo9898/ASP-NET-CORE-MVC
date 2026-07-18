using AHUWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Article> Articles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<GroupPermission> GroupPermissions { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public DbSet<Staff> Staffs { get; set; } = null!;
        public DbSet<Feedback> Feedbacks { get; set; } = null!;
        public DbSet<FeedbackMessage> FeedbackMessages { get; set; } = null!;
        public DbSet<ForumPost> ForumPosts { get; set; } = null!;
        public DbSet<ForumImage> ForumImages { get; set; } = null!;
        public DbSet<ForumReaction> ForumReactions { get; set; } = null!;
        public DbSet<ForumComment> ForumComments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FeedbackMessage>()
                .HasOne(m => m.Feedback)
                .WithMany(f => f.Messages)
                .HasForeignKey(m => m.FeedbackId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Diễn đàn (Forum) =====
            modelBuilder.Entity<ForumPost>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ForumImage>()
                .HasOne(i => i.ForumPost)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ForumPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumReaction>()
                .HasOne(r => r.ForumPost)
                .WithMany(p => p.Reactions)
                .HasForeignKey(r => r.ForumPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumReaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Mỗi user chỉ 1 reaction/bài — đổi loại reaction thì UPDATE dòng này,
            // không insert dòng mới (xử lý ở Controller, Pha 4).
            modelBuilder.Entity<ForumReaction>()
                .HasIndex(r => new { r.ForumPostId, r.UserId })
                .IsUnique();

            modelBuilder.Entity<ForumComment>()
                .HasOne(c => c.ForumPost)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.ForumPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumComment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===== Lab 01: Category cha-con, Group/Permission/GroupPermission =====
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupPermission>()
                .HasOne(gp => gp.Group)
                .WithMany(g => g.GroupPermissions)
                .HasForeignKey(gp => gp.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupPermission>()
                .HasOne(gp => gp.Permission)
                .WithMany(p => p.GroupPermissions)
                .HasForeignKey(gp => gp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Lab 08: User <-> Group (tuy chon, khong thay the cot User.Role) =====
            modelBuilder.Entity<User>()
                .HasOne(u => u.Group)
                .WithMany(g => g.Users)
                .HasForeignKey(u => u.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===== Lab 09: du lieu mau cho Group/Permission/GroupPermission qua HasData =====
            // Id co dinh (bat buoc voi HasData) - chi la du lieu minh hoa, khong anh huong
            // toi tai khoan admin mac dinh (van GroupId = null, khong bi gioi han - xem Lab 08).
            modelBuilder.Entity<Group>().HasData(
                new Group { Id = 1, Name = "Toàn quyền", Description = "Được thao tác mọi chức năng trong khu vực quản trị" },
                new Group { Id = 2, Name = "Biên tập viên", Description = "Chỉ quản lý sản phẩm và tin tức, không quản lý nhóm quyền/người dùng" }
            );

            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Code = "Group.Manage", Name = "Quản lý nhóm quyền" },
                new Permission { Id = 2, Code = "Product.Manage", Name = "Quản lý sản phẩm" },
                new Permission { Id = 3, Code = "Article.Manage", Name = "Quản lý tin tức" },
                new Permission { Id = 4, Code = "Order.Manage", Name = "Quản lý đơn hàng" },
                new Permission { Id = 5, Code = "User.Manage", Name = "Quản lý người dùng" }
            );

            modelBuilder.Entity<GroupPermission>().HasData(
                new GroupPermission { Id = 1, GroupId = 1, PermissionId = 1 },
                new GroupPermission { Id = 2, GroupId = 1, PermissionId = 2 },
                new GroupPermission { Id = 3, GroupId = 1, PermissionId = 3 },
                new GroupPermission { Id = 4, GroupId = 1, PermissionId = 4 },
                new GroupPermission { Id = 5, GroupId = 1, PermissionId = 5 },
                new GroupPermission { Id = 6, GroupId = 2, PermissionId = 2 },
                new GroupPermission { Id = 7, GroupId = 2, PermissionId = 3 }
            );

            // NOTE: the default admin account is NOT seeded here via HasData, because
            // HasData needs a fixed, pre-computed password hash baked into a migration.
            // Instead it's created at startup by Data/DbInitializer.cs using BCrypt.Net,
            // so the hash is always generated correctly at runtime. See Program.cs.
        }
    }
}
