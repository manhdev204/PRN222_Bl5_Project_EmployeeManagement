using Microsoft.AspNetCore.Mvc;
using PRN222_BL5_Project_EmployeeManagement.Models;
using System;
using System.Linq;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
	public class AttendanceController : Controller
	{
		private readonly Prn222Bl5ProjectEmployeeManagementContext _context;

		public AttendanceController(Prn222Bl5ProjectEmployeeManagementContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			int accountId = 1; // TODO: lấy từ session login
			var today = DateOnly.FromDateTime(DateTime.Today);

			var attendance = _context.Attendances
				.FirstOrDefault(a => a.AccountId == accountId && a.AttendanceDate == today);

			return View(attendance);
		}

		[HttpPost]
		public IActionResult CheckIn()
		{
			int accountId = 1; // TODO: lấy từ session login
			var today = DateOnly.FromDateTime(DateTime.Today);

			var attendance = _context.Attendances
				.FirstOrDefault(a => a.AccountId == accountId && a.AttendanceDate == today);

			if (attendance == null)
			{
				attendance = new Attendance
				{
					AccountId = accountId,
					AttendanceDate = today,
					CheckInTime = DateTime.Now,
					Status = DateTime.Now.Hour > 8 ? 2 : 1, // đi muộn nếu sau 8h
					OnLeave = 0,
					CreatedDate = DateTime.Now
				};
				_context.Attendances.Add(attendance);
			}
			else if (attendance.CheckInTime == null)
			{
				attendance.CheckInTime = DateTime.Now;
				attendance.Status = DateTime.Now.Hour > 8 ? 2 : 1;
				attendance.LastUpdatedDate = DateTime.Now;
			}

			_context.SaveChanges();
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult CheckOut()
		{
			int accountId = 1; // TODO: lấy từ session login
			var today = DateOnly.FromDateTime(DateTime.Today);

			var attendance = _context.Attendances
				.FirstOrDefault(a => a.AccountId == accountId && a.AttendanceDate == today);

			if (attendance != null && attendance.CheckOutTime == null)
			{
				attendance.CheckOutTime = DateTime.Now;
				attendance.LastUpdatedDate = DateTime.Now;
				_context.SaveChanges();
			}

			return RedirectToAction("Index");
		}
	}
}
