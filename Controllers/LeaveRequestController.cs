using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PRN222_BL5_Project_EmployeeManagement.Models;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
    public class LeaveRequestController : Controller
    {
        private readonly Prn222Bl5ProjectEmployeeManagementContext _db;

        private const string SessionKeyUserId = "AUTH_USER_ID";
        private const string SessionKeyRole = "AUTH_ROLE";
        private const int ROLE_EMPLOYEE = 1;
        private const int ROLE_MANAGER = 2;
        private const int ROLE_ADMIN = 3;

        public LeaveRequestController(Prn222Bl5ProjectEmployeeManagementContext db)
        {
            _db = db;
        }

        private int? CurrentUserId()
        {
            return HttpContext.Session.GetInt32(SessionKeyUserId);
        }

        private int? CurrentRole()
        {
            var s = HttpContext.Session.GetString(SessionKeyRole);
            if (int.TryParse(s, out var r)) return r;
            return null;
        }

        private IActionResult? GuardRequireLogin()
        {
            var uid = CurrentUserId();
            if (!uid.HasValue)
                return RedirectToAction("Login", "Authentication", new { returnUrl = Url.Action("Create", "LeaveRequest") });
            return null;
        }

        private IActionResult? GuardManagerOrAdmin()
        {
            var uid = CurrentUserId();
            if (!uid.HasValue)
                return RedirectToAction("Login", "Authentication", new { returnUrl = Url.Action("ApproveList", "LeaveRequest") });
            var role = CurrentRole() ?? 0;
            if (role != ROLE_MANAGER && role != ROLE_ADMIN)
                return StatusCode(403);
            return null;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var g = GuardRequireLogin();
            if (g != null) return g;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DateOnly startDate, DateOnly endDate, string leaveReason)
        {
            var g = GuardRequireLogin();
            if (g != null) return g;

            if (endDate < startDate)
            {
                ViewBag.Error = "Ngày kết thúc phải sau ngày bắt đầu.";
                return View();
            }
            if (string.IsNullOrWhiteSpace(leaveReason))
            {
                ViewBag.Error = "Vui lòng nhập lý do.";
                return View();
            }

            var req = new LeaveRequest
            {
                AccountId = CurrentUserId()!.Value,
                StartDate = startDate,
                EndDate = endDate,
                LeaveReason = leaveReason.Trim(),
                Status = 1,
                CreatedDate = DateTime.Now,
                DeleteFlag = false
            };
            _db.LeaveRequests.Add(req);
            _db.SaveChanges();

            TempData["Success"] = "Đã gửi đơn nghỉ.";
            return RedirectToAction(nameof(MyRequests));
        }

        [HttpGet]
        public IActionResult MyRequests()
        {
            var g = GuardRequireLogin();
            if (g != null) return g;

            var uid = CurrentUserId()!.Value;
            var list = _db.LeaveRequests
                .Where(x => x.AccountId == uid && (x.DeleteFlag == null || x.DeleteFlag == false))
                .OrderByDescending(x => x.CreatedDate)
                .ToList();
            return View(list);
        }

        [HttpGet]
        public IActionResult ApproveList(int? status, int? accountId, DateOnly? from, DateOnly? to, string? q)
        {
            var g = GuardManagerOrAdmin();
            if (g != null) return g;

            var query = _db.LeaveRequests
                .Include(l => l.Account)
                .Where(l => l.DeleteFlag == null || l.DeleteFlag == false)
                .AsQueryable();

            if (status.HasValue) query = query.Where(l => l.Status == status.Value);
            if (accountId.HasValue) query = query.Where(l => l.AccountId == accountId.Value);
            if (from.HasValue) query = query.Where(l => l.StartDate >= from.Value);
            if (to.HasValue) query = query.Where(l => l.EndDate <= to.Value);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim();
                query = query.Where(l =>
                    (l.Account.FullName ?? "").Contains(s) ||
                    (l.Account.Username ?? "").Contains(s) ||
                    (l.LeaveReason ?? "").Contains(s));
            }

            var list = query
                .OrderBy(l => l.Status)
                .ThenByDescending(l => l.CreatedDate)
                .ToList();

            ViewBag.Status = status;
            ViewBag.Accounts = _db.Accounts.OrderBy(a => a.FullName).ToList();
            ViewBag.SelectedAccountId = accountId;
            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.Q = q;

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var g = GuardManagerOrAdmin();
            if (g != null) return g;

            var approverId = CurrentUserId() ?? 0;
            var req = _db.LeaveRequests.FirstOrDefault(l => l.LeaveRequestId == id);
            if (req == null) return RedirectToAction(nameof(ApproveList));
            if (req.Status != 1)
            {
                TempData["Error"] = "Chỉ xử lý đơn đang chờ.";
                return RedirectToAction(nameof(ApproveList), new { status = 1 });
            }
            req.Status = 2;
            req.LastUpdatedDate = DateTime.Now;
            req.ApprovedId = approverId;
            _db.SaveChanges();
            TempData["Success"] = "Đã duyệt đơn.";
            return RedirectToAction(nameof(ApproveList), new { status = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            var g = GuardManagerOrAdmin();
            if (g != null) return g;

            var approverId = CurrentUserId() ?? 0;
            var req = _db.LeaveRequests.FirstOrDefault(l => l.LeaveRequestId == id);
            if (req == null) return RedirectToAction(nameof(ApproveList));
            if (req.Status != 1)
            {
                TempData["Error"] = "Chỉ xử lý đơn đang chờ.";
                return RedirectToAction(nameof(ApproveList), new { status = 1 });
            }
            req.Status = 3;
            req.LastUpdatedDate = DateTime.Now;
            req.ApprovedId = approverId;
            _db.SaveChanges();
            TempData["Success"] = "Đã từ chối đơn.";
            return RedirectToAction(nameof(ApproveList), new { status = 1 });
        }
    }
}
