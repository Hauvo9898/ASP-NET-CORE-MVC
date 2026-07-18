using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AHUWeb.Models
{
    // Lab 01/08/09: "Nhóm quyền" — tách biệt với cột User.Role (admin/customer) đang dùng cho
    // đăng nhập/Cookie Auth thật. Gán User vào 1 Group (thêm ở Lab 08) chỉ để giới hạn quyền
    // chi tiết hơn bên trong khu vực admin, không thay thế cơ chế Role hiện có.
    public class Group : IAuditable
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
        public ICollection<User> Users { get; set; } = new List<User>();

        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
