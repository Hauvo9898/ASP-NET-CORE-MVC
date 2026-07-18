using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AHUWeb.Helpers
{
    // Lab 08: kiem tra ma quyen (permission code) luu trong Session, doc lap voi
    // Cookie Authentication dang dung that (chi la 1 lop kiem soat chi tiet hon
    // BEN TRONG khu vuc Admin, khong thay the [Authorize(Roles="admin")] dang bao
    // ve toan bo khu vuc). Neu tai khoan chua duoc gan vao Group nao (Session
    // khong co danh sach ma quyen) thi coi nhu khong bi gioi han - dam bao tai
    // khoan admin mac dinh dang dung khong bi anh huong.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        public const string PermissionsSessionKey = "PermissionCodes";

        private readonly string _code;

        public RequirePermissionAttribute(string code)
        {
            _code = code;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var codesRaw = context.HttpContext.Session.GetString(PermissionsSessionKey);
            if (codesRaw == null) return; // chua gan Group -> khong gioi han

            var codes = codesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (!codes.Contains(_code))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
