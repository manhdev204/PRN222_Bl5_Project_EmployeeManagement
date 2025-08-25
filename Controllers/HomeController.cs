using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
	public class HomeController : Controller
	{
		private const string SessionKeyUserId = "AUTH_USER_ID";
		private const string SessionKeyRole = "AUTH_ROLE";

		private const int ROLE_ADMIN = 3;
		private const int ROLE_MANAGER = 2;
		private const int ROLE_EMPLOYEE = 1;

		public IActionResult Index()
		{
			var uid = HttpContext.Session.GetInt32(SessionKeyUserId);
			if (!uid.HasValue) return View();

			var roleStr = HttpContext.Session.GetString(SessionKeyRole);
			int.TryParse(roleStr, out var roleId);

            if (roleId == ROLE_ADMIN)
                return RedirectToAction("Index", "AdminAccounts");

            if (roleId == ROLE_MANAGER)
				return RedirectToAction("Index", "ManagerHome");

			if (roleId == ROLE_EMPLOYEE)
				return RedirectToAction("Index", "EmployeeHome");

			return View();
		}
	}
}
