using System;
using System.Collections.Generic;

namespace PRN222_BL5_Project_EmployeeManagement.Models;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public int? CreatedId { get; set; }

    public DateTime? LastUpdatedDate { get; set; }

    public int? LastUpdatedId { get; set; }

    public bool? DeleteFlag { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
