using System;
using System.Collections.Generic;

namespace PRN222_BL5_Project_EmployeeManagement.Models.ViewModels
{
	public class TeamAttendanceRowVm
	{
		public int AccountId { get; set; }
		public string FullName { get; set; } = "";
		public string DepartmentName { get; set; } = "";
		public DateTime? CheckIn { get; set; }
		public DateTime? CheckOut { get; set; }
		public int Status { get; set; }          // 0 = absent, 1 = present, 2 = late
		public bool OnLeave { get; set; }        // true nếu đang nghỉ phép (approved)
		public string StatusText { get; set; } = "";
		public string StatusBadge { get; set; } = "bg-secondary";
		public bool HasAttendance { get; set; }  // có record attendance trong ngày
	}

	public class ManagerAttendanceVm
	{
		public DateOnly Date { get; set; }
		public int? DepartmentId { get; set; }
		public string? Search { get; set; }

		public List<TeamAttendanceRowVm> Rows { get; set; } = new();

		// KPI nhanh
		public int PresentCount { get; set; }
		public int LateCount { get; set; }
		public int AbsentCount { get; set; }
		public int OnLeaveCount { get; set; }

		// Dropdown phòng ban
		public List<(int id, string name)> Departments { get; set; } = new();
	}
}
