using Microsoft.AspNetCore.Mvc;
using PRN222_BL5_Project_EmployeeManagement.Models;
using PRN222_BL5_Project_EmployeeManagement.Filters;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
    [AdminOnly]
    public class AdminDepartmentsController : Controller
    {
        private readonly Prn222Bl5ProjectEmployeeManagementContext _db;

        public AdminDepartmentsController(Prn222Bl5ProjectEmployeeManagementContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var departments = _db.Departments.OrderBy(d => d.DepartmentId).ToList();
            return View(departments);
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


