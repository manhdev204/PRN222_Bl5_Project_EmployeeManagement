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

        [HttpGet("/AdminAccounts")]
        public IActionResult Index(string? username, string? fullName, string? email, string? phone, int? departmentId, int? roleId, string? sortField, string? sortDir)
        {
            var query = _db.Accounts
                .Include(a => a.Role)
                .Include(a => a.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(username))
            {
                var like = username.Trim();
                query = query.Where(a => a.Username != null && a.Username.Contains(like));
            }
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var like = fullName.Trim();
                query = query.Where(a => a.FullName != null && a.FullName.Contains(like));
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                var like = email.Trim();
                query = query.Where(a => a.Email != null && a.Email.Contains(like));
            }
            if (!string.IsNullOrWhiteSpace(phone))
            {
                var like = phone.Trim();
                query = query.Where(a => a.Phone != null && a.Phone.Contains(like));
            }
            if (departmentId.HasValue)
            {
                query = query.Where(a => a.DepartmentId == departmentId.Value);
            }
            if (roleId.HasValue)
            {
                query = query.Where(a => a.RoleId == roleId.Value);
            }

            var field = (sortField ?? "account_id").ToLowerInvariant();
            var dir = (sortDir ?? "asc").ToLowerInvariant();
            bool desc = dir == "desc";

            query = field switch
            {
                "username" => (desc ? query.OrderByDescending(a => a.Username) : query.OrderBy(a => a.Username)),
                "created_date" => (desc ? query.OrderByDescending(a => a.CreatedDate) : query.OrderBy(a => a.CreatedDate)),
                "last_updated_date" => (desc ? query.OrderByDescending(a => a.LastUpdatedDate) : query.OrderBy(a => a.LastUpdatedDate)),
                _ => (desc ? query.OrderByDescending(a => a.AccountId) : query.OrderBy(a => a.AccountId))
            };

            ViewBag.Username = username;
            ViewBag.FullName = fullName;
            ViewBag.Email = email;
            ViewBag.Phone = phone;
            ViewBag.Departments = _db.Departments.OrderBy(d => d.DepartmentName).ToList();
            ViewBag.Roles = _db.Roles.OrderBy(r => r.RoleName).ToList();
            ViewBag.SelectedDepartmentId = departmentId;
            ViewBag.SelectedRoleId = roleId;
            ViewBag.SortField = field;
            ViewBag.SortDir = dir;

            var accounts = query.OrderBy(a => a.AccountId).ToList();
            return View(accounts);
        }

        [HttpGet("/AdminAccounts/Create")]
        public IActionResult Create()
        {
            ViewBag.Roles = _db.Roles.OrderBy(r => r.RoleName).ToList();
            ViewBag.Departments = _db.Departments.OrderBy(d => d.DepartmentName).ToList();
            return View();
        }

        [HttpPost("/AdminAccounts/Create")]
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

        [HttpGet("/AdminAccounts/Edit/{id}")]
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

        [HttpPost("/AdminAccounts/Edit")]
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


