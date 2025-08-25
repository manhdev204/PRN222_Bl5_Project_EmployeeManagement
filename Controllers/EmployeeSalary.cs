using Microsoft.AspNetCore.Mvc;
using PRN222_BL5_Project_EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
    public class EmployeeSalary : Controller
    {
        Prn222Bl5ProjectEmployeeManagementContext _context = new Prn222Bl5ProjectEmployeeManagementContext();

      public  EmployeeSalary(Prn222Bl5ProjectEmployeeManagementContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            int accountId = 3; // TODO: Lấy từ user đăng nhập (employee)
            var salary = _context.Salaries.Include(s => s.Account).Where(s => s.AccountId == accountId && (s.DeleteFlag == false || s.DeleteFlag == null)).ToList();
            return View(salary);
        }
    }
}
