using AHUWeb.Data;
using AHUWeb.Helpers;
using AHUWeb.Models;
using AHUWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Areas.Admin.Controllers
{
    // Lab 04+05: quan ly "Nhom quyen" bang Ajax DataTables (server-side) + Modal CRUD.
    // Entity moi hoan toan (xem Lab 01) nen an toan tuyet doi - khong dung cham gi den
    // Product/User/Order dang chay that.
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    [RequirePermission("Group.Manage")]
    public class GroupsController : AdminBaseController
    {
        private readonly ApplicationDbContext _db;

        public GroupsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Admin/Groups - khung trang, du lieu bang duoc JS goi qua GetDataTable
        public async Task<IActionResult> Index()
        {
            // Lab 09: danh sach quyen tinh, dung de ve checkbox grid trong modal Sua/Them
            ViewBag.AllPermissions = await _db.Permissions.OrderBy(p => p.Id)
                .Select(p => new { p.Id, p.Code, p.Name })
                .ToListAsync();
            return View();
        }

        // POST /Admin/Groups/GetDataTable - JQuery DataTables server-side processing (Lab 04)
        [HttpPost]
        public async Task<IActionResult> GetDataTable([FromForm] JDataTableRequest request)
        {
            var query = _db.Groups.AsQueryable();
            var recordsTotal = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                var s = request.SearchValue.Trim();
                query = query.Where(g => g.Name.Contains(s) || (g.Description != null && g.Description.Contains(s)));
            }

            var recordsFiltered = await query.CountAsync();

            bool asc = request.SortDirection == "asc";
            query = request.SortColumn switch
            {
                "Name" => asc ? query.OrderBy(g => g.Name) : query.OrderByDescending(g => g.Name),
                "Description" => asc ? query.OrderBy(g => g.Description) : query.OrderByDescending(g => g.Description),
                _ => query.OrderByDescending(g => g.Id)
            };

            var data = await query
                .Skip(request.Start)
                .Take(request.Length <= 0 ? 10 : request.Length)
                .Select(g => new { g.Id, g.Name, g.Description })
                .ToListAsync();

            return Json(new
            {
                draw = request.Draw,
                recordsTotal,
                recordsFiltered,
                data
            });
        }

        // GET /Admin/Groups/GetById/5 - Ajax lay du lieu de gan vao Form Modal khi Sua (Lab 05),
        // kem danh sach permissionIds da gan cho nhom (Lab 09) de tick san checkbox.
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var group = await _db.Groups.FindAsync(id);
            if (group == null) return NotFound();

            var permissionIds = await _db.GroupPermissions
                .Where(gp => gp.GroupId == id)
                .Select(gp => gp.PermissionId)
                .ToListAsync();

            return Json(new { group.Id, group.Name, group.Description, permissionIds });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupFormViewModel model, List<int>? permissionIds)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Vui lòng nhập tên nhóm quyền." });

            var group = new Group
            {
                Name = model.Name,
                Description = model.Description,
                CreatedBy = User.Identity?.Name,
                CreatedOn = DateTime.Now
            };
            _db.Groups.Add(group);
            await _db.SaveChangesAsync();

            await SyncPermissions(group.Id, permissionIds);

            return Json(new { success = true, message = "Đã thêm nhóm quyền." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupFormViewModel model, List<int>? permissionIds)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Vui lòng nhập tên nhóm quyền." });

            var group = await _db.Groups.FindAsync(model.Id);
            if (group == null)
                return Json(new { success = false, message = "Không tìm thấy nhóm quyền." });

            group.Name = model.Name;
            group.Description = model.Description;
            group.ModifiedBy = User.Identity?.Name;
            group.ModifiedOn = DateTime.Now;
            await _db.SaveChangesAsync();

            await SyncPermissions(group.Id, permissionIds);

            return Json(new { success = true, message = "Đã cập nhật nhóm quyền." });
        }

        // Lab 09: dong bo danh sach quyen cua 1 nhom - xoa het roi chen lai danh sach da chon,
        // don gian va an toan voi so luong quyen nho nhu hien tai.
        private async Task SyncPermissions(int groupId, List<int>? permissionIds)
        {
            var current = _db.GroupPermissions.Where(gp => gp.GroupId == groupId);
            _db.GroupPermissions.RemoveRange(current);

            if (permissionIds != null)
            {
                foreach (var pid in permissionIds.Distinct())
                    _db.GroupPermissions.Add(new GroupPermission { GroupId = groupId, PermissionId = pid });
            }

            await _db.SaveChangesAsync();
        }

        // POST /Admin/Groups/Delete/5 - thay cho confirmDelete() bang SweetAlert2 (Lab 05)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _db.Groups.FindAsync(id);
            if (group == null)
                return Json(new { success = false, message = "Không tìm thấy nhóm quyền." });

            _db.Groups.Remove(group);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa nhóm quyền." });
        }
    }
}
