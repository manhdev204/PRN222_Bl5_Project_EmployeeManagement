using System;
using System.Collections.Generic;

namespace PRN222_BL5_Project_EmployeeManagement.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public int? DepartmentId { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int RoleId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedId { get; set; }

    public DateTime? LastUpdatedDate { get; set; }

    public int? LastUpdatedId { get; set; }

    public bool? DeleteFlag { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<LeaveRequest> LeaveRequestAccounts { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<LeaveRequest> LeaveRequestApproveds { get; set; } = new List<LeaveRequest>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();
}
