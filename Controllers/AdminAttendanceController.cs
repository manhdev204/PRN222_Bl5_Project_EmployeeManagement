using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222_BL5_Project_EmployeeManagement.Models;

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
    public class AdminAttendanceController : Controller
    {
        private readonly Prn222Bl5ProjectEmployeeManagementContext _db;

        public AdminAttendanceController(Prn222Bl5ProjectEmployeeManagementContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index(string? username, string? fullName, DateTime? fromDate, DateTime? toDate, string? status, int? departmentId, string? sortField, string? sortDir)
        {
            var query = _db.Attendances
                .Include(a => a.Account)
                .ThenInclude(acc => acc.Department)
                .AsQueryable();

            // Search filters
            if (!string.IsNullOrWhiteSpace(username))
            {
                var like = username.Trim();
                query = query.Where(a => a.Account.Username != null && a.Account.Username.Contains(like));
            }
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var like = fullName.Trim();
                query = query.Where(a => a.Account.FullName != null && a.Account.FullName.Contains(like));
            }
            if (fromDate.HasValue)
            {
                query = query.Where(a => a.AttendanceDate >= DateOnly.FromDateTime(fromDate.Value));
            }
            if (toDate.HasValue)
            {
                query = query.Where(a => a.AttendanceDate <= DateOnly.FromDateTime(toDate.Value));
            }
            if (!string.IsNullOrWhiteSpace(status) && int.TryParse(status, out int statusValue))
            {
                query = query.Where(a => a.Status == statusValue);
            }
            if (departmentId.HasValue)
            {
                query = query.Where(a => a.Account.DepartmentId == departmentId.Value);
            }

            // Sorting
            var field = (sortField ?? "attendance_date").ToLowerInvariant();
            var dir = (sortDir ?? "desc").ToLowerInvariant();
            bool desc = dir == "desc";

            query = field switch
            {
                "username" => (desc ? query.OrderByDescending(a => a.Account.Username) : query.OrderBy(a => a.Account.Username)),
                "full_name" => (desc ? query.OrderByDescending(a => a.Account.FullName) : query.OrderBy(a => a.Account.FullName)),
                "check_in" => (desc ? query.OrderByDescending(a => a.CheckInTime) : query.OrderBy(a => a.CheckInTime)),
                "check_out" => (desc ? query.OrderByDescending(a => a.CheckOutTime) : query.OrderBy(a => a.CheckOutTime)),
                "created_date" => (desc ? query.OrderByDescending(a => a.CreatedDate) : query.OrderBy(a => a.CreatedDate)),
                _ => (desc ? query.OrderByDescending(a => a.AttendanceDate) : query.OrderBy(a => a.AttendanceDate))
            };

            ViewBag.Username = username;
            ViewBag.FullName = fullName;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;
            ViewBag.Departments = _db.Departments.OrderBy(d => d.DepartmentName).ToList();
            ViewBag.SelectedDepartmentId = departmentId;
            ViewBag.SortField = field;
            ViewBag.SortDir = dir;

            var attendances = query.ToList();
            return View(attendances);
        }
    }
}
