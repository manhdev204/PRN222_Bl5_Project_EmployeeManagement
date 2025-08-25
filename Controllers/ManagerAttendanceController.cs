using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PRN222_BL5_Project_EmployeeManagement.Models;
using PRN222_BL5_Project_EmployeeManagement.Models.ViewModels;
using System;
using System.Linq;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
	public class ManagerAttendanceController : Controller
	{
		private readonly Prn222Bl5ProjectEmployeeManagementContext _context;

		private const string SessionKeyUserId = "AUTH_USER_ID";
		private const string SessionKeyRole = "AUTH_ROLE";

		private const int ROLE_MANAGER = 2;
		private const int ROLE_ADMIN = 3;

		private static readonly TimeSpan WorkStart = new TimeSpan(9, 0, 0);
		private const int LateGraceMinutes = 15;

		public ManagerAttendanceController(Prn222Bl5ProjectEmployeeManagementContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Requires login and role ∈ {Manager, Admin}.
		/// - Not logged in: redirect to /Authentication/Login?returnUrl=...
		/// - Logged in with wrong role: return 403 + TempData error.
		/// Returns (userId, roleId) if valid; otherwise returns IActionResult for caller to return immediately.
		/// </summary>
		private (int userId, int roleId)? RequireManagerOrAdmin(out IActionResult? failResult)
		{
			failResult = null;

			var uid = HttpContext.Session.GetInt32(SessionKeyUserId);
			if (!uid.HasValue)
			{
				var returnUrl = Url.Action("Today", "ManagerAttendance",
					new { date = Request.Query["date"].ToString(), departmentId = Request.Query["departmentId"].ToString(), q = Request.Query["q"].ToString() });
				failResult = RedirectToAction("Login", "Authentication", new { returnUrl });
				return null;
			}

			var roleStr = HttpContext.Session.GetString(SessionKeyRole);
			if (!int.TryParse(roleStr, out var roleId) || (roleId != ROLE_MANAGER && roleId != ROLE_ADMIN))
			{
				TempData["Error"] = "You do not have permission to access this screen.";
				failResult = StatusCode(403);
				return null;
			}

			return (uid.Value, roleId);
		}

		[HttpGet]
		public IActionResult Today(DateOnly? date, int? departmentId, string? q)
		{
			var auth = RequireManagerOrAdmin(out var fail);
			if (auth == null) return fail!;

			var theDay = date ?? DateOnly.FromDateTime(DateTime.Today);
			var vm = new ManagerAttendanceVm
			{
				Date = theDay,
				DepartmentId = departmentId,
				Search = q
			};

			vm.Departments = _context.Departments
				.Where(d => d.DeleteFlag == null || d.DeleteFlag == false)
				.OrderBy(d => d.DepartmentName)
				.Select(d => new ValueTuple<int, string>(d.DepartmentId, d.DepartmentName))
				.ToList();

			var accountsQuery = _context.Accounts
				.Where(a => a.DeleteFlag == null || a.DeleteFlag == false);

			if (departmentId.HasValue)
				accountsQuery = accountsQuery.Where(a => a.DepartmentId == departmentId.Value);

			if (!string.IsNullOrWhiteSpace(q))
				accountsQuery = accountsQuery.Where(a =>
					(a.FullName ?? "").Contains(q) ||
					(a.Username ?? "").Contains(q) ||
					(a.Email ?? "").Contains(q));

			var accounts = accountsQuery
				.Select(a => new
				{
					a.AccountId,
					a.FullName,
					DepartmentName = a.Department != null ? a.Department.DepartmentName : ""
				})
				.ToList();

			var accIds = accounts.Select(a => a.AccountId).ToList();

			var todayAtt = _context.Attendances
				.Where(t => t.AttendanceDate == theDay && accIds.Contains(t.AccountId))
				.ToList()
				.ToDictionary(t => t.AccountId, t => t);

			var leaveToday = _context.LeaveRequests
				.Where(l => l.Status == 2 && accIds.Contains(l.AccountId)
							&& l.StartDate <= theDay && l.EndDate >= theDay)
				.Select(l => new { l.AccountId })
				.Distinct()
				.ToList()
				.ToDictionary(x => x.AccountId, x => true);

			foreach (var a in accounts)
			{
				todayAtt.TryGetValue(a.AccountId, out var att);
				var onLeave = leaveToday.ContainsKey(a.AccountId) || (att?.OnLeave == 1);

				var row = new TeamAttendanceRowVm
				{
					AccountId = a.AccountId,
					FullName = a.FullName ?? "(No name)",
					DepartmentName = a.DepartmentName ?? "",
					CheckIn = att?.CheckInTime,
					CheckOut = att?.CheckOutTime,
					Status = att?.Status ?? 0,
					OnLeave = onLeave,
					HasAttendance = att != null
				};

				if (onLeave)
				{
					row.StatusText = "On Leave";
					row.StatusBadge = "bg-info text-dark";
				}
				else if (att == null || att.CheckInTime == null)
				{
					row.StatusText = "Not checked in";
					row.StatusBadge = "bg-secondary";
				}
				else
				{
					if (att.Status == 2)
					{
						row.StatusText = att.CheckOutTime == null ? "Late (working)" : "Late";
						row.StatusBadge = "bg-warning text-dark";
					}
					else if (att.Status == 1)
					{
						row.StatusText = att.CheckOutTime == null ? "Present (working)" : "Present";
						row.StatusBadge = "bg-success";
					}
					else
					{
						row.StatusText = "Absent";
						row.StatusBadge = "bg-secondary";
					}
				}

				vm.Rows.Add(row);
			}

			vm.PresentCount = vm.Rows.Count(r => r.Status == 1 && r.CheckIn != null && !r.OnLeave);
			vm.LateCount = vm.Rows.Count(r => r.Status == 2 && !r.OnLeave);
			vm.OnLeaveCount = vm.Rows.Count(r => r.OnLeave);
			vm.AbsentCount = vm.Rows.Count(r => !r.HasAttendance && !r.OnLeave);

			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult MarkAbsent(int accountId, DateOnly date, bool excused)
		{
			var auth = RequireManagerOrAdmin(out var fail);
			if (auth == null) return fail!;

			var att = _context.Attendances
				.FirstOrDefault(a => a.AccountId == accountId && a.AttendanceDate == date);

			if (att == null)
			{
				att = new Attendance
				{
					AccountId = accountId,
					AttendanceDate = date,
					Status = 0,                // absent
					OnLeave = excused ? 1 : 0,
					CreatedDate = DateTime.Now
				};
				_context.Attendances.Add(att);
			}
			else
			{
				if (att.CheckInTime != null)
				{
					TempData["Error"] = "Employee already checked in. Cannot mark absent.";
					return RedirectToAction(nameof(Today), new { date, departmentId = (int?)null });
				}

				att.Status = 0;
				att.OnLeave = excused ? 1 : 0;
				att.LastUpdatedDate = DateTime.Now;
			}

			_context.SaveChanges();
			TempData["Success"] = excused ? "Marked as excused absence." : "Marked as unexcused absence.";
			return RedirectToAction(nameof(Today), new { date });
		}
	}
}
