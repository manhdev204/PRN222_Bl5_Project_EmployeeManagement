using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace PRN222_BL5_Project_EmployeeManagement.Models;

public partial class Salary
{
    public int SalaryId { get; set; }

    public int AccountId { get; set; }

    public decimal BaseSalary { get; set; }

    public decimal? Bonus { get; set; }

    public decimal? TotalSalary { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedId { get; set; }

    public DateTime? LastUpdatedDate { get; set; }

    public int? LastUpdatedId { get; set; }

    public bool? DeleteFlag { get; set; }

    [ValidateNever]
    public virtual Account Account { get; set; } = null!;
}
