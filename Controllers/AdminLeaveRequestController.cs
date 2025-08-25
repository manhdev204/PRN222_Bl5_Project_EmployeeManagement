using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222_BL5_Project_EmployeeManagement.Models;
 

namespace PRN222_BL5_Project_EmployeeManagement.Controllers
{
    public class AdminLeaveRequestController : Controller
    {
        private readonly Prn222Bl5ProjectEmployeeManagementContext _db;

        public AdminLeaveRequestController(Prn222Bl5ProjectEmployeeManagementContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index(string? username, string? fullName, DateTime? fromDate, DateTime? toDate, int? status, int? departmentId, string? sortField, string? sortDir)
        {
            var query = _db.LeaveRequests
                .Include(l => l.Account)
                .ThenInclude(acc => acc.Department)
                .AsQueryable();

            // Search filters
            if (!string.IsNullOrWhiteSpace(username))
            {
                var like = username.Trim();
                query = query.Where(l => l.Account.Username != null && l.Account.Username.Contains(like));
            }
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var like = fullName.Trim();
                query = query.Where(l => l.Account.FullName != null && l.Account.FullName.Contains(like));
            }
            if (fromDate.HasValue)
            {
                query = query.Where(l => l.StartDate >= DateOnly.FromDateTime(fromDate.Value));
            }
            if (toDate.HasValue)
            {
                query = query.Where(l => l.EndDate <= DateOnly.FromDateTime(toDate.Value));
            }
            if (status.HasValue)
            {
                query = query.Where(l => l.Status == status.Value);
            }
            if (departmentId.HasValue)
            {
                query = query.Where(l => l.Account.DepartmentId == departmentId.Value);
            }

            // Sorting
            var field = (sortField ?? "start_date").ToLowerInvariant();
            var dir = (sortDir ?? "desc").ToLowerInvariant();
            bool desc = dir == "desc";

            query = field switch
            {
                "username" => (desc ? query.OrderByDescending(l => l.Account.Username) : query.OrderBy(l => l.Account.Username)),
                "full_name" => (desc ? query.OrderByDescending(l => l.Account.FullName) : query.OrderBy(l => l.Account.FullName)),
                "end_date" => (desc ? query.OrderByDescending(l => l.EndDate) : query.OrderBy(l => l.EndDate)),
                "status" => (desc ? query.OrderByDescending(l => l.Status) : query.OrderBy(l => l.Status)),
                "created_date" => (desc ? query.OrderByDescending(l => l.CreatedDate) : query.OrderBy(l => l.CreatedDate)),
                _ => (desc ? query.OrderByDescending(l => l.StartDate) : query.OrderBy(l => l.StartDate))
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

            var leaveRequests = query.ToList();
            return View(leaveRequests);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var leaveRequest = _db.LeaveRequests
                .Include(l => l.Account)
                .FirstOrDefault(l => l.LeaveRequestId == id);
            if (leaveRequest == null)
            {
                return RedirectToAction("Index");
            }
            return View(leaveRequest);
        }

        [HttpPost]
        public IActionResult Edit(int id, DateOnly startDate, DateOnly endDate, string? leaveReason, int status, bool? deleteFlag)
        {
            var leaveRequest = _db.LeaveRequests.FirstOrDefault(l => l.LeaveRequestId == id);
            if (leaveRequest == null)
            {
                return RedirectToAction("Index");
            }

            // Only allow editing if status is still pending (1)
            if (leaveRequest.Status != 1)
            {
                TempData["Error"] = "Cannot edit leave request that is already approved or rejected.";
                return RedirectToAction("Index");
            }

            // Only allow status changes to approved (2) or rejected (3)
            if (status != 2 && status != 3)
            {
                TempData["Error"] = "Status can only be changed to Approved or Rejected.";
                return RedirectToAction("Index");
            }

            leaveRequest.StartDate = startDate;
            leaveRequest.EndDate = endDate;
            leaveRequest.LeaveReason = leaveReason;
            leaveRequest.Status = status;
            leaveRequest.DeleteFlag = deleteFlag ?? false;
            leaveRequest.LastUpdatedDate = DateTime.UtcNow;
            _db.SaveChanges();
            
            TempData["Success"] = $"Leave request status updated to {(status == 2 ? "Approved" : "Rejected")}.";
            return RedirectToAction("Index");
        }
    }
}
