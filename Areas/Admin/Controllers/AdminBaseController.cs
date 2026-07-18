using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AHUWeb.Areas.Admin.Controllers
{
    public abstract class AdminBaseController : Controller
    {
        private static readonly Dictionary<string, string> TabMap = new()
        {
            ["Dashboard"] = "overview",
            ["Products"] = "products",
            ["Categories"] = "categories",
            ["Groups"] = "groups",
            ["Orders"] = "orders",
            ["Users"] = "users",
            ["Articles"] = "articles",
            ["Staffs"] = "staffs",
            ["Feedbacks"] = "contacts",
            ["Forum"] = "forum"
        };

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "";
            var actionName = context.RouteData.Values["action"]?.ToString() ?? "";

            // Dashboard/Reports dùng tab "reports" thay vì "overview"
            ViewData["ActiveTab"] = (controllerName == "Dashboard" && actionName == "Reports")
                ? "reports"
                : TabMap.GetValueOrDefault(controllerName, "overview");

            base.OnActionExecuting(context);
        }
    }
}
