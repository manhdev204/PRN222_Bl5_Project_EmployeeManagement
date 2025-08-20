using System;
using System.Collections.Generic;

namespace PRN222_BL5_Project_EmployeeManagement.Models;

public partial class LeaveRequest
{
    public int LeaveRequestId { get; set; }

    public int AccountId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? LeaveReason { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedId { get; set; }

    public DateTime? LastUpdatedDate { get; set; }

    public int? LastUpdatedId { get; set; }

    public bool? DeleteFlag { get; set; }

    public virtual Account Account { get; set; } = null!;
}
