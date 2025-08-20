using System;
using System.Collections.Generic;

namespace PRN222_BL5_Project_EmployeeManagement.Models;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int AccountId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public int Status { get; set; }

    public int OnLeave { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedId { get; set; }

    public DateTime? LastUpdatedDate { get; set; }

    public int? LastUpdatedId { get; set; }

    public bool? DeleteFlag { get; set; }

    public virtual Account Account { get; set; } = null!;
}
