namespace AHUWeb.Models
{
    // Lab 01: chuẩn hóa thông tin SEO cho các entity hiển thị công khai.
    public interface IMeta
    {
        string? MetaKeyword { get; set; }
        string? MetaDescription { get; set; }
    }
}
