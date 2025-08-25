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

        // List salary
        public IActionResult Index()
        {
            var salaryList = _context.Salaries
                                     .Include(s => s.Account)
                                     .Where(s => s.DeleteFlag == false || s.DeleteFlag == null)
                                     .ToList();
            return View(salaryList);
        }

        // GET: Create
        public IActionResult Create()
        {
        
            ViewBag.Accounts = new SelectList(_context.Accounts
                                                        .Where(a => a.RoleId == 3 && (a.DeleteFlag == false || a.DeleteFlag == null))
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
                salary.TotalSalary = salary.BaseSalary + (salary.Bonus ?? 0);
                salary.CreatedDate = DateTime.Now;
                salary.CreatedId = 1; // TODO: Lấy từ user đăng nhập (manager)
                salary.Account = _context.Accounts.First(a => a.AccountId == salary.AccountId);
                _context.Salaries.Add(salary);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Accounts = new SelectList(_context.Accounts
                                                  .Where(a => a.RoleId == 3 && (a.DeleteFlag == false || a.DeleteFlag == null))
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
                var existing = _context.Salaries.Find(salary.SalaryId);
                if (existing == null) return NotFound();

                existing.BaseSalary = salary.BaseSalary;
                existing.Bonus = salary.Bonus;
                existing.TotalSalary = salary.BaseSalary + (salary.Bonus ?? 0);
                existing.LastUpdatedDate = DateTime.Now;
                existing.LastUpdatedId = 1; // TODO: lấy từ manager login

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
