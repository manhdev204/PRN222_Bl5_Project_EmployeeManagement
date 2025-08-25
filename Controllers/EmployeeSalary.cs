using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using PRN222_BL5_Project_EmployeeManagement.Models;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
    public class EmployeeSalary : Controller
    {
        Prn222Bl5ProjectEmployeeManagementContext _context = new Prn222Bl5ProjectEmployeeManagementContext();

        public EmployeeSalary(Prn222Bl5ProjectEmployeeManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index(string sortOrder, string search, DateTime? startDate, DateTime? endDate)
        {
            var accountId = HttpContext.Session.GetInt32("AUTH_USER_ID");
            if (!accountId.HasValue)
            {
                return RedirectToAction("Login", "Authentication");
            }
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentSearch"] = search;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

            var salaries = _context.Salaries.Include(s => s.Account).Where(s => s.AccountId == accountId && (s.DeleteFlag == false || s.DeleteFlag == null)).AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(search))
            {
                salaries = salaries.Where(s => s.Account.FullName.Contains(search)
                                            || s.BaseSalary.ToString().Contains(search)
                                            || s.Bonus.ToString().Contains(search)
                                            || s.Deduction.ToString().Contains(search)
                                            || s.TotalSalary.ToString().Contains(search)
                                            );
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
            return View(salaries.ToList());
        }
    }

}
