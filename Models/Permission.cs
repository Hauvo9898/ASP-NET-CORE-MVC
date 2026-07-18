using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AHUWeb.Models
{
    // Lab 01/08/09: mã quyền cho từng hành động (VD "Group.Manage"). Tương đương "Role" trong
    // giáo trình gốc — đổi tên thành Permission để không trùng cột User.Role (admin/customer).
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
    }
}
