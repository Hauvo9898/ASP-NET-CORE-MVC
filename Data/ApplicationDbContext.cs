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

            // NOTE: the default admin account is NOT seeded here via HasData, because
            // HasData needs a fixed, pre-computed password hash baked into a migration.
            // Instead it's created at startup by Data/DbInitializer.cs using BCrypt.Net,
            // so the hash is always generated correctly at runtime. See Program.cs.
        }
    }
}
