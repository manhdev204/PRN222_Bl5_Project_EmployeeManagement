using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PRN222_BL5_Project_EmployeeManagement.Models;
using System;
using System.Linq;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
	public class AttendanceController : Controller
	{
		private readonly Prn222Bl5ProjectEmployeeManagementContext _context;

		private const string SessionKeyUserId = "AUTH_USER_ID";
		private const string SessionKeyRole = "AUTH_ROLE";

		private static readonly TimeSpan WorkStart = new TimeSpan(9, 0, 0);   // 09:00
		private const int LateGraceMinutes = 15;                               // cho phép muộn 15'

		public AttendanceController(Prn222Bl5ProjectEmployeeManagementContext context)
		{
			_context = context;
		}

		private int? TryGetSessionAccountIdOrRedirect()
		{
			var uid = HttpContext.Session.GetInt32(SessionKeyUserId);
			if (!uid.HasValue)
			{
				var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
				Response.Redirect(Url.Action("Login", "Authentication", new { returnUrl })!);
				return null;
			}
			return uid.Value;
		}

		public IActionResult Index()
		{
			var accountId = TryGetSessionAccountIdOrRedirect();
			if (!accountId.HasValue) return new EmptyResult(); 

			var today = DateOnly.FromDateTime(DateTime.Today);

			var attendance = _context.Attendances
				.FirstOrDefault(a => a.AccountId == accountId.Value && a.AttendanceDate == today);

			if (attendance == null && IsOnLeaveToday(accountId.Value, today))
			{
				attendance = new Attendance
				{
					AccountId = accountId.Value,
					AttendanceDate = today,
					Status = 0,
					OnLeave = 1
				};
			}

			return View(attendance);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CheckIn()
		{
			var accountId = TryGetSessionAccountIdOrRedirect();
			if (!accountId.HasValue) return new EmptyResult();

			var today = DateOnly.FromDateTime(DateTime.Today);
			var now = DateTime.Now;

			if (IsOnLeaveToday(accountId.Value, today))
			{
				TempData["Error"] = "Hôm nay bạn đang trong kỳ nghỉ đã duyệt, không thể check-in.";
				return RedirectToAction(nameof(Index));
			}

			var attendance = _context.Attendances
				.FirstOrDefault(a => a.AccountId == accountId.Value && a.AttendanceDate == today);

			// Tính muộn theo giờ chuẩn + grace
			var workStartToday = now.Date + WorkStart;
			bool isLate = now > workStartToday.AddMinutes(LateGraceMinutes);

			if (attendance == null)
			{
				attendance = new Attendance
				{
					AccountId = accountId.Value,
					AttendanceDate = today,
					CheckInTime = now,
					Status = isLate ? 2 : 1,  // 1 present, 2 late
					OnLeave = 0,
					CreatedDate = now
				};
				_context.Attendances.Add(attendance);
				_context.SaveChanges();
				TempData["Success"] = $"Check-in lúc {now:HH:mm}.";
				return RedirectToAction(nameof(Index));
			}

			if (attendance.OnLeave == 1)
			{
				TempData["Error"] = "Bản ghi hôm nay là ngày nghỉ, không thể check-in.";
				return RedirectToAction(nameof(Index));
			}

			if (attendance.CheckInTime != null)
			{
				TempData["Error"] = $"Bạn đã check-in lúc {attendance.CheckInTime:HH:mm}.";
				return RedirectToAction(nameof(Index));
			}

			attendance.CheckInTime = now;
			attendance.Status = isLate ? 2 : 1;
			attendance.LastUpdatedDate = now;
			_context.SaveChanges();

			TempData["Success"] = $"Check-in lúc {now:HH:mm}.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CheckOut()
		{
			var accountId = TryGetSessionAccountIdOrRedirect();
			if (!accountId.HasValue) return new EmptyResult();

			var today = DateOnly.FromDateTime(DateTime.Today);
			var now = DateTime.Now;

			var attendance = _context.Attendances
				.FirstOrDefault(a => a.AccountId == accountId.Value && a.AttendanceDate == today);

			if (attendance == null || attendance.CheckInTime == null)
			{
				TempData["Error"] = "Bạn chưa check-in hôm nay.";
				return RedirectToAction(nameof(Index));
			}

			if (attendance.OnLeave == 1)
			{
				TempData["Error"] = "Hôm nay là ngày nghỉ, không thể check-out.";
				return RedirectToAction(nameof(Index));
			}

			if (attendance.CheckOutTime != null)
			{
				TempData["Error"] = $"Bạn đã check-out lúc {attendance.CheckOutTime:HH:mm}.";
				return RedirectToAction(nameof(Index));
			}

			if (now < attendance.CheckInTime)
			{
				TempData["Error"] = "Thời gian check-out không hợp lệ.";
				return RedirectToAction(nameof(Index));
			}

			attendance.CheckOutTime = now;
			attendance.LastUpdatedDate = now;
			_context.SaveChanges();

			TempData["Success"] = $"Check-out lúc {now:HH:mm}.";
			return RedirectToAction(nameof(Index));
		}

		private bool IsOnLeaveToday(int accountId, DateOnly day)
		{
			return _context.LeaveRequests.Any(l =>
				l.AccountId == accountId &&
				l.Status == 1 &&
				l.StartDate <= day &&
				l.EndDate >= day
			);
		}
	}
}
