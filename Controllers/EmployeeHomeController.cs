using Microsoft.AspNetCore.Mvc;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
    public class EmployeeHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
