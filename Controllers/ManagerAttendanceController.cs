using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN222_BL5_Project_EmployeeManagement.Models;
using PRN222_BL5_Project_EmployeeManagement.Models.ViewModels;
using System;
using System.Linq;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
	public class ManagerAttendanceController : Controller
	{
		private readonly Prn222Bl5ProjectEmployeeManagementContext _context;

		// cấu hình giờ vào + grace
		private static readonly TimeSpan WorkStart = new TimeSpan(9, 0, 0);
		private const int LateGraceMinutes = 15;
		private int GetCurrentAccountId() => 1; // nếu cần phân quyền, thay bằng claims/session

		public ManagerAttendanceController(Prn222Bl5ProjectEmployeeManagementContext context)
		{
			_context = context;
		}

		[HttpGet]
		public IActionResult Today(DateOnly? date, int? departmentId, string? q)
		{
			var theDay = date ?? DateOnly.FromDateTime(DateTime.Today);
			var vm = new ManagerAttendanceVm
			{
				Date = theDay,
				DepartmentId = departmentId,
				Search = q
			};

			// danh sách phòng ban cho dropdown
			vm.Departments = _context.Departments
				.Where(d => d.DeleteFlag == null || d.DeleteFlag == false)
				.OrderBy(d => d.DepartmentName)
				.Select(d => new ValueTuple<int, string>(d.DepartmentId, d.DepartmentName))
				.ToList();

			// lấy danh sách account theo filter
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

			// attendance trong ngày
			var todayAtt = _context.Attendances
				.Where(t => t.AttendanceDate == theDay && accIds.Contains(t.AccountId))
				.ToList()
				.ToDictionary(t => t.AccountId, t => t);

			// nghỉ phép đã duyệt trong ngày
			var leaveToday = _context.LeaveRequests
				.Where(l => l.Status == 1 && accIds.Contains(l.AccountId)
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

				// xác định text/badge
				if (onLeave)
				{
					row.StatusText = "Nghỉ phép";
					row.StatusBadge = "bg-info text-dark";
				}
				else if (att == null)
				{
					row.StatusText = "Chưa check-in";
					row.StatusBadge = "bg-secondary";
				}
				else
				{
					if (att.CheckInTime == null)
					{
						row.StatusText = "Chưa check-in";
						row.StatusBadge = "bg-secondary";
					}
					else
					{
						if (att.Status == 2)
						{
							row.StatusText = att.CheckOutTime == null ? "Đi muộn (đang làm)" : "Đi muộn";
							row.StatusBadge = "bg-warning text-dark";
						}
						else if (att.Status == 1)
						{
							row.StatusText = att.CheckOutTime == null ? "Đi làm (đang làm)" : "Đi làm";
							row.StatusBadge = "bg-success";
						}
						else
						{
							row.StatusText = "Nghỉ";
							row.StatusBadge = "bg-secondary";
						}
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

		// quick action: đánh dấu vắng (có/không lý do)
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult MarkAbsent(int accountId, DateOnly date, bool excused)
		{
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
				// nếu đã có check-in thì không đổi sang vắng
				if (att.CheckInTime != null)
				{
					TempData["Error"] = "Nhân viên đã check-in, không thể đánh dấu vắng.";
					return RedirectToAction(nameof(Today), new { date, departmentId = (int?)null });
				}

				att.Status = 0;
				att.OnLeave = excused ? 1 : 0;
				att.LastUpdatedDate = DateTime.Now;
			}

			_context.SaveChanges();
			TempData["Success"] = excused ? "Đã đánh dấu vắng có lý do." : "Đã đánh dấu vắng không lý do.";
			return RedirectToAction(nameof(Today), new { date });
		}
	}
}
