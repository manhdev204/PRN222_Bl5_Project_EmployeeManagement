using Microsoft.AspNetCore.Mvc;
using PRN222_BL5_Project_EmployeeManagement.Models;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
    public class AdminDepartmentsController : Controller
    {
        private readonly Prn222Bl5ProjectEmployeeManagementContext _db;

        public AdminDepartmentsController(Prn222Bl5ProjectEmployeeManagementContext db)
        {
            _db = db;
        }

        public IActionResult Index(string? q, string? sortField, string? sortDir)
        {
            var query = _db.Departments.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var like = q.Trim();
                query = query.Where(d => d.DepartmentName != null && d.DepartmentName.Contains(like));
            }

            var field = (sortField ?? "department_id").ToLowerInvariant();
            var dir = (sortDir ?? "asc").ToLowerInvariant();
            bool desc = dir == "desc";

            query = field switch
            {
                "created_date" => (desc ? query.OrderByDescending(d => d.CreatedDate) : query.OrderBy(d => d.CreatedDate)),
                "updated_date" => (desc ? query.OrderByDescending(d => d.LastUpdatedDate) : query.OrderBy(d => d.LastUpdatedDate)),
                _ => (desc ? query.OrderByDescending(d => d.DepartmentId) : query.OrderBy(d => d.DepartmentId))
            };

            ViewBag.Query = q;
            ViewBag.SortField = field;
            ViewBag.SortDir = dir;
            return View(query.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Department());
        }

        [HttpPost]
        public IActionResult Create(Department model)
        {
            if (string.IsNullOrWhiteSpace(model.DepartmentName))
            {
                ViewBag.Error = "Department name is required.";
                return View(model);
            }
            model.CreatedDate = DateTime.UtcNow;
            model.DeleteFlag = model.DeleteFlag ?? false;
            _db.Departments.Add(model);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var department = _db.Departments.FirstOrDefault(d => d.DepartmentId == id);
            if (department == null)
            {
                return RedirectToAction("Index");
            }
            return View(department);
        }

        [HttpPost]
        public IActionResult Edit(int id, string departmentName, bool? deleteFlag)
        {
            var department = _db.Departments.FirstOrDefault(d => d.DepartmentId == id);
            if (department == null)
            {
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                ViewBag.Error = "Department name is required.";
                return View(department);
            }
            department.DepartmentName = departmentName;
            department.DeleteFlag = deleteFlag ?? false;
            department.LastUpdatedDate = DateTime.UtcNow;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}


