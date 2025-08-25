using Microsoft.AspNetCore.Mvc;
using PRN222_BL5_Project_EmployeeManagement.Models;
using PRN222_BL5_Project_EmployeeManagement.Filters;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
    [AdminOnly]
    public class AdminRolesController : Controller
    {
        private readonly Prn222Bl5ProjectEmployeeManagementContext _db;

        public AdminRolesController(Prn222Bl5ProjectEmployeeManagementContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var roles = _db.Roles.OrderBy(r => r.RoleId).ToList();
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Role());
        }

        [HttpPost]
        public IActionResult Create(Role model)
        {
            if (string.IsNullOrWhiteSpace(model.RoleName))
            {
                ViewBag.Error = "Role name is required.";
                return View(model);
            }
            if (_db.Roles.Any(r => r.RoleName == model.RoleName))
            {
                ViewBag.Error = "Role name already exists.";
                return View(model);
            }
            model.CreatedDate = DateTime.UtcNow;
            model.DeleteFlag = model.DeleteFlag ?? false;
            _db.Roles.Add(model);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var role = _db.Roles.FirstOrDefault(r => r.RoleId == id);
            if (role == null)
            {
                return RedirectToAction("Index");
            }
            return View(role);
        }

        [HttpPost]
        public IActionResult Edit(int id, string roleName, bool? deleteFlag)
        {
            var role = _db.Roles.FirstOrDefault(r => r.RoleId == id);
            if (role == null)
            {
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ViewBag.Error = "Role name is required.";
                return View(role);
            }
            // prevent duplicate names
            if (_db.Roles.Any(r => r.RoleName == roleName && r.RoleId != id))
            {
                ViewBag.Error = "Role name already exists.";
                return View(role);
            }
            role.RoleName = roleName;
            role.DeleteFlag = deleteFlag ?? false;
            role.LastUpdatedDate = DateTime.UtcNow;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}


