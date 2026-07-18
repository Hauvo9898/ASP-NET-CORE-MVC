using System;

namespace AHUWeb.Models
{
    // Lab 01: chuẩn hóa thông tin người/thời điểm tạo-sửa cho các entity cần audit trail.
    public interface IAuditable
    {
        string? CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string? ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}
