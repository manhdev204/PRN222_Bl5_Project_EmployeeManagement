using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
	public class ManagerHomeController : Controller
	{
		private const string SessionKeyUserId = "AUTH_USER_ID";
		private const string SessionKeyRole = "AUTH_ROLE";
		private const int ROLE_MANAGER = 2;
		private const int ROLE_ADMIN = 3;

		private IActionResult? Guard()
		{
			var uid = HttpContext.Session.GetInt32(SessionKeyUserId);
			if (!uid.HasValue)
				return RedirectToAction("Login", "Authentication");
			var roleStr = HttpContext.Session.GetString(SessionKeyRole);
			if (!int.TryParse(roleStr, out var role) || (role != ROLE_MANAGER && role != ROLE_ADMIN))
				return StatusCode(403);
			return null;
		}

		public IActionResult Index()
		{
			var g = Guard();
			if (g != null) return g;
			return View();
		}
	}
}
