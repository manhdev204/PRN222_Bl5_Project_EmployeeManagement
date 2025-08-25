using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PRN222_BL5_Project_EmployeeManagement.Models;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly Prn222Bl5ProjectEmployeeManagementContext _db;

        public AuthenticationController(Prn222Bl5ProjectEmployeeManagementContext db)
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

        private const string SessionKeyUserId = "AUTH_USER_ID";
        private const string SessionKeyUsername = "AUTH_USERNAME";
        private const string SessionKeyRole = "AUTH_ROLE";
        private const string TempVerifyCodePrefix = "VERIFY_CODE_";
        private const string TempResetCodePrefix = "RESET_CODE_";

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            var user = _db.Accounts.FirstOrDefault(a => a.Username == username && a.DeleteFlag != true);
            if (user == null)
            {
                ViewBag.Error = "Invalid credentials.";
                return View();
            }

            var hashed = HashPassword(password);
            var matchesHashed = string.Equals(user.Password, hashed, StringComparison.OrdinalIgnoreCase);
            if (!matchesHashed)
            {
                ViewBag.Error = "Invalid credentials.";
                return View();
            }

            HttpContext.Session.SetInt32(SessionKeyUserId, user.AccountId);
            HttpContext.Session.SetString(SessionKeyUsername, user.Username);
            HttpContext.Session.SetString(SessionKeyRole, user.RoleId.ToString());

            if (user.RoleId == 1)
            {
                return RedirectToAction("Index", "AdminAccounts");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password, string confirmPassword, string? email, string? fullName)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }
            if (!string.Equals(password, confirmPassword))
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }
            var exists = _db.Accounts.Any(a => a.Username == username);
            if (exists)
            {
                ViewBag.Error = "Username already exists.";
                return View();
            }

            var defaultRoleId = _db.Roles.OrderBy(r => r.RoleId).Select(r => r.RoleId).FirstOrDefault();
            if (defaultRoleId == 0)
            {
                defaultRoleId = 3;
            }

            var account = new Account
            {
                Username = username,
                Password = HashPassword(password),
                Email = email,
                FullName = fullName,
                RoleId = defaultRoleId,
                CreatedDate = DateTime.UtcNow,
                DeleteFlag = false
            };
            _db.Accounts.Add(account);
            _db.SaveChanges();

            // Generate simple verification code and store in TempData (no email service per requirement)
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            TempData[TempVerifyCodePrefix + account.AccountId] = code;
            ViewBag.VerifyCode = code; // show code to user to simulate email

            return RedirectToAction("VerifyEmail", new { id = account.AccountId });
        }

        [HttpGet]
        public IActionResult VerifyEmail(int id)
        {
            ViewBag.AccountId = id;
            ViewBag.Code = TempData.Peek(TempVerifyCodePrefix + id)?.ToString();
            return View();
        }

        [HttpPost]
        public IActionResult VerifyEmail(int id, string code)
        {
            var expected = TempData.Peek(TempVerifyCodePrefix + id)?.ToString();
            if (string.IsNullOrEmpty(expected))
            {
                ViewBag.Error = "Verification expired. Please re-register or contact support.";
                ViewBag.AccountId = id;
                return View();
            }
            if (!string.Equals(code, expected, StringComparison.Ordinal))
            {
                ViewBag.Error = "Invalid code.";
                ViewBag.AccountId = id;
                ViewBag.Code = expected;
                return View();
            }

            // Mark verified: simplest is to set LastUpdatedDate as a flag, or repurpose. Better to have a field, but not present.
            var account = _db.Accounts.FirstOrDefault(a => a.AccountId == id);
            if (account != null)
            {
                account.LastUpdatedDate = DateTime.UtcNow;
                _db.SaveChanges();
            }
            TempData.Remove(TempVerifyCodePrefix + id);

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!HttpContext.Session.GetInt32(SessionKeyUserId).HasValue)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeyUserId);
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "New password must be at least 6 characters.";
                return View();
            }
            if (!string.Equals(newPassword, confirmPassword))
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }
            var user = _db.Accounts.FirstOrDefault(a => a.AccountId == userId.Value);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            if (!string.Equals(user.Password, HashPassword(currentPassword), StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Current password is incorrect.";
                return View();
            }
            user.Password = HashPassword(newPassword);
            user.LastUpdatedDate = DateTime.UtcNow;
            _db.SaveChanges();

            // Force re-login after password change
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string usernameOrEmail)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
            {
                ViewBag.Error = "Please enter your username or email.";
                return View();
            }
            var user = _db.Accounts.FirstOrDefault(a => (a.Username == usernameOrEmail || a.Email == usernameOrEmail) && a.DeleteFlag != true);
            if (user == null)
            {
                ViewBag.Error = "Account not found.";
                return View();
            }
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            TempData[TempResetCodePrefix + user.AccountId] = code;
            // For demo: show code on next page
            return RedirectToAction("ResetPassword", new { id = user.AccountId });
        }

        [HttpGet]
        public IActionResult ResetPassword(int id)
        {
            ViewBag.AccountId = id;
            ViewBag.Code = TempData.Peek(TempResetCodePrefix + id)?.ToString();
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(int id, string code, string newPassword, string confirmPassword)
        {
            var expected = TempData.Peek(TempResetCodePrefix + id)?.ToString();
            if (string.IsNullOrEmpty(expected))
            {
                ViewBag.Error = "Reset code expired. Please try again.";
                ViewBag.AccountId = id;
                return View();
            }
            if (!string.Equals(code, expected, StringComparison.Ordinal))
            {
                ViewBag.Error = "Invalid code.";
                ViewBag.AccountId = id;
                ViewBag.Code = expected;
                return View();
            }
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "New password must be at least 6 characters.";
                ViewBag.AccountId = id;
                ViewBag.Code = expected;
                return View();
            }
            if (!string.Equals(newPassword, confirmPassword))
            {
                ViewBag.Error = "Passwords do not match.";
                ViewBag.AccountId = id;
                ViewBag.Code = expected;
                return View();
            }
            var user = _db.Accounts.FirstOrDefault(a => a.AccountId == id);
            if (user == null)
            {
                ViewBag.Error = "Account not found.";
                ViewBag.AccountId = id;
                return View();
            }
            user.Password = HashPassword(newPassword);
            user.LastUpdatedDate = DateTime.UtcNow;
            _db.SaveChanges();
            TempData.Remove(TempResetCodePrefix + id);
            return RedirectToAction("Login");
        }
    }
}


