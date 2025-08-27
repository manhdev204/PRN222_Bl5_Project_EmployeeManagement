using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRN222_BL5_Project_EmployeeManagement.Models;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
    public class ManagerSalaryController : Controller
    {
        private readonly Prn222Bl5ProjectEmployeeManagementContext _context;

        public ManagerSalaryController(Prn222Bl5ProjectEmployeeManagementContext context)
        {
            _context = context;
        }

        public IActionResult Home()
        {
            return View();
        }

        // List salary


        public IActionResult Index(string sortOrder, string search, DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 3)
        {
            var userId = HttpContext.Session.GetInt32("AUTH_USER_ID");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Authentication");
            }
            var manager = _context.Accounts.Include(a => a.Department).FirstOrDefault(a => a.AccountId == userId.Value);

            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentSearch"] = search;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

            var salaries = _context.Salaries
                .Include(s => s.Account)
                .Where(s => s.Account.DepartmentId == manager.DepartmentId
                         && (s.DeleteFlag == false || s.DeleteFlag == null))
                .AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(search))
            {
                salaries = salaries.Where(s => s.Account.FullName.Contains(search)
                                            || s.BaseSalary.ToString().Contains(search)
                                            || s.Bonus.ToString().Contains(search)
                                            || s.Deduction.ToString().Contains(search)
                                            || s.TotalSalary.ToString().Contains(search));
            }
            if (startDate.HasValue)
                salaries = salaries.Where(s => s.CreatedDate >= startDate);
            if (endDate.HasValue)
                salaries = salaries.Where(s => s.CreatedDate <= endDate);

            // Sorting
            salaries = sortOrder switch
            {
                "name_desc" => salaries.OrderByDescending(s => s.Account.FullName),
                "name_asc" => salaries.OrderBy(s => s.Account.FullName),

                "salary_desc" => salaries.OrderByDescending(s => s.BaseSalary),
                "salary_asc" => salaries.OrderBy(s => s.BaseSalary),

                "bonus_desc" => salaries.OrderByDescending(s => s.Bonus),
                "bonus_asc" => salaries.OrderBy(s => s.Bonus),

                "deduction_desc" => salaries.OrderByDescending(s => s.Deduction),
                "deduction_asc" => salaries.OrderBy(s => s.Deduction),

                "total_desc" => salaries.OrderByDescending(s => s.TotalSalary),
                "total_asc" => salaries.OrderBy(s => s.TotalSalary),

                "date_desc" => salaries.OrderByDescending(s => s.CreatedDate),
                "date_asc" => salaries.OrderBy(s => s.CreatedDate),

                _ => salaries.OrderBy(s => s.Account.FullName),
            };

            // Pagination
            int totalItems = salaries.Count();
            var items = salaries.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(items);
        }

        // GET: Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("AUTH_USER_ID");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Authentication");
            }
            var manager = _context.Accounts.Include(a => a.Department).FirstOrDefault(a => a.AccountId == userId.Value);

            var employeesInDept = _context.Accounts
                                          .Where(a => a.DepartmentId == manager.DepartmentId && a.RoleId == 1 && (a.DeleteFlag == false || a.DeleteFlag == null))
                                          .Select(a => a.AccountId)
                                          .ToList();
            ViewBag.Accounts = new SelectList(employeesInDept == null ? new List<Account>() : _context.Accounts
                                                        .Where(a => employeesInDept.Contains(a.AccountId))
                                                        .Select(a => new { a.AccountId, a.FullName })
                                                        .ToList(), "AccountId", "FullName");
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Salary salary)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("AUTH_USER_ID");
                if (!userId.HasValue)
                {
                    return RedirectToAction("Login", "Authentication");
                }
                salary.TotalSalary = salary.BaseSalary + (salary.Bonus ?? 0) - (salary.Deduction ?? 0);
                salary.CreatedDate = DateTime.Now;
                salary.CreatedId = userId.Value;
                salary.Account = _context.Accounts.First(a => a.AccountId == salary.AccountId);

                _context.Salaries.Add(salary);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Accounts = new SelectList(_context.Accounts
                                                  .Where(a => a.RoleId == 1 && (a.DeleteFlag == false || a.DeleteFlag == null))
                                                  .Select(a => new { a.AccountId, a.FullName })
                                                  .ToList(), "AccountId", "FullName", salary.AccountId);
            return View(salary);
        }

        // GET: Edit
        public IActionResult Edit(int id)
        {
            var salary = _context.Salaries.Find(id);
            if (salary == null) return NotFound();
            return View(salary);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Salary salary)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("AUTH_USER_ID"); // lấy từ session
                if (!userId.HasValue)
                {
                    return RedirectToAction("Login", "Authentication");
                }
                var existing = _context.Salaries.Find(salary.SalaryId);
                if (existing == null) return NotFound();
                existing.BaseSalary = salary.BaseSalary;
                existing.Bonus = salary.Bonus;
                existing.Deduction = salary.Deduction;
                existing.TotalSalary = salary.BaseSalary + (salary.Bonus ?? 0) - (salary.Deduction ?? 0);
                existing.LastUpdatedDate = DateTime.Now;
                existing.LastUpdatedId = userId.Value;

                _context.Update(existing);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(salary);
        }

        // GET: Delete
        public IActionResult Delete(int id)
        {
            var salary = _context.Salaries
                                 .Include(s => s.Account)
                                 .FirstOrDefault(s => s.SalaryId == id);
            if (salary == null) return NotFound();
            return View(salary);
        }

        // POST: Delete Confirm
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var salary = _context.Salaries.Find(id);
            if (salary == null) return NotFound();

            salary.DeleteFlag = true;
            _context.Update(salary);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
