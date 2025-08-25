using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRN222_BL5_Project_EmployeeManagement.Models;

public partial class Salary
{
    public int SalaryId { get; set; }

    public int AccountId { get; set; }
    [Required(ErrorMessage = "Base Salary is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Base Salary must be greater than 0")]
    public decimal BaseSalary { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Bonus must be greater than 0")]
    public decimal? Bonus { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Deduction must be greater than 0")]
    public decimal? Deduction { get; set; }

    public decimal? TotalSalary { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedId { get; set; }

    public DateTime? LastUpdatedDate { get; set; }

    public int? LastUpdatedId { get; set; }

    public bool? DeleteFlag { get; set; }

    [ValidateNever]
    public virtual Account Account { get; set; } = null!;
}
