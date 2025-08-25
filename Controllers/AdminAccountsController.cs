using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222_BL5_Project_EmployeeManagement.Models;
using System.Security.Cryptography;
using System.Text;
using PRN222_BL5_Project_EmployeeManagement.Filters;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
    [AdminOnly]
    public class AdminAccountsController : Controller
    {
        private readonly Prn222Bl5ProjectEmployeeManagementContext _db;

        public AdminAccountsController(Prn222Bl5ProjectEmployeeManagementContext db)
        {
            _db = db;
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        public IActionResult Index()
        {
            var accounts = _db.Accounts
                .Include(a => a.Role)
                .Include(a => a.Department)
                .OrderBy(a => a.AccountId)
                .ToList();
            return View(accounts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = _db.Roles.OrderBy(r => r.RoleName).ToList();
            ViewBag.Departments = _db.Departments.OrderBy(d => d.DepartmentName).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Account model, string? plainPassword)
        {
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ViewBag.Error = "Username is required.";
                ViewBag.Roles = _db.Roles.OrderBy(r => r.RoleName).ToList();
                ViewBag.Departments = _db.Departments.OrderBy(d => d.DepartmentName).ToList();
                return View(model);
            }
            if (_db.Accounts.Any(a => a.Username == model.Username))
            {
                ViewBag.Error = "Username already exists.";
                ViewBag.Roles = _db.Roles.OrderBy(r => r.RoleName).ToList();
                ViewBag.Departments = _db.Departments.OrderBy(d => d.DepartmentName).ToList();
                return View(model);
            }

            model.Password = HashPassword(string.IsNullOrEmpty(plainPassword) ? "123456" : plainPassword);
            model.CreatedDate = DateTime.UtcNow;
            model.DeleteFlag = model.DeleteFlag ?? false;
            _db.Accounts.Add(model);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var account = _db.Accounts.FirstOrDefault(a => a.AccountId == id);
            if (account == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Roles = _db.Roles.OrderBy(r => r.RoleName).ToList();
            ViewBag.Departments = _db.Departments.OrderBy(d => d.DepartmentName).ToList();
            return View(account);
        }

        [HttpPost]
        public IActionResult Edit(int id, int roleId, int? departmentId, bool? deleteFlag)
        {
            var account = _db.Accounts.FirstOrDefault(a => a.AccountId == id);
            if (account == null)
            {
                return RedirectToAction("Index");
            }
            account.RoleId = roleId;
            account.DepartmentId = departmentId;
            account.DeleteFlag = deleteFlag ?? false;
            account.LastUpdatedDate = DateTime.UtcNow;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}


