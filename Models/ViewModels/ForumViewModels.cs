using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AHUWeb.Models.ViewModels
{
    public class ForumPostFormViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập nội dung bài viết")]
        [StringLength(2000, ErrorMessage = "Tối đa 2000 ký tự")]
        public string Content { get; set; } = string.Empty;

        // Cho chọn nhiều ảnh 1 lúc (input multiple ở view)
        public List<IFormFile>? ImageFiles { get; set; }
    }
}
