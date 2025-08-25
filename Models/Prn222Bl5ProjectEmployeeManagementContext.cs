using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PRN222_BL5_Project_EmployeeManagement.Models;

public partial class Prn222Bl5ProjectEmployeeManagementContext : DbContext
{
    public Prn222Bl5ProjectEmployeeManagementContext()
    {
    }

    public Prn222Bl5ProjectEmployeeManagementContext(DbContextOptions<Prn222Bl5ProjectEmployeeManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Salary> Salaries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:MyCnn");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__46A222CDACAEF877");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Username, "UQ__Account__F3DBC5727CF2CF88").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.CreatedId).HasColumnName("created_id");
            entity.Property(e => e.DeleteFlag)
                .HasDefaultValue(false)
                .HasColumnName("delete_flag");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("full_name");
            entity.Property(e => e.LastUpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("last_updated_date");
            entity.Property(e => e.LastUpdatedId).HasColumnName("last_updated_id");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("username");

            entity.HasOne(d => d.Department).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_account_department");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Role");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__20D6A968FEB43379");

            entity.ToTable("Attendance");

            entity.Property(e => e.AttendanceId).HasColumnName("attendance_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");
            entity.Property(e => e.CheckInTime)
                .HasColumnType("datetime")
                .HasColumnName("check_in_time");
            entity.Property(e => e.CheckOutTime)
                .HasColumnType("datetime")
                .HasColumnName("check_out_time");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.CreatedId).HasColumnName("created_id");
            entity.Property(e => e.DeleteFlag)
                .HasDefaultValue(false)
                .HasColumnName("delete_flag");
            entity.Property(e => e.LastUpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("last_updated_date");
            entity.Property(e => e.LastUpdatedId).HasColumnName("last_updated_id");
            entity.Property(e => e.OnLeave).HasColumnName("on_leave");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Account).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attendance_Account");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__C2232422CF5D28EB");

            entity.ToTable("Department");

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.CreatedId).HasColumnName("created_id");
            entity.Property(e => e.DeleteFlag)
                .HasDefaultValue(false)
                .HasColumnName("delete_flag");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("department_name");
            entity.Property(e => e.LastUpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("last_updated_date");
            entity.Property(e => e.LastUpdatedId).HasColumnName("last_updated_id");
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.LeaveRequestId).HasName("PK__Leave_Re__F42B99E8A7F2F001");

            entity.ToTable("Leave_Request");

            entity.Property(e => e.LeaveRequestId).HasColumnName("leave_request_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.CreatedId).HasColumnName("created_id");
            entity.Property(e => e.DeleteFlag)
                .HasDefaultValue(false)
                .HasColumnName("delete_flag");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.LastUpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("last_updated_date");
            entity.Property(e => e.LastUpdatedId).HasColumnName("last_updated_id");
            entity.Property(e => e.LeaveReason)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("leave_reason");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Account).WithMany(p => p.LeaveRequests)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveRequest_Account");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__760965CC97C23745");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.CreatedId).HasColumnName("created_id");
            entity.Property(e => e.DeleteFlag)
                .HasDefaultValue(false)
                .HasColumnName("delete_flag");
            entity.Property(e => e.LastUpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("last_updated_date");
            entity.Property(e => e.LastUpdatedId).HasColumnName("last_updated_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Salary>(entity =>
        {
            entity.HasKey(e => e.SalaryId).HasName("PK__Salary__A3C71C51DAA9AB9E");

            entity.ToTable("Salary");

            entity.Property(e => e.SalaryId).HasColumnName("salary_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.BaseSalary)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("base_salary");
            entity.Property(e => e.Bonus)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("bonus");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.CreatedId).HasColumnName("created_id");
            entity.Property(e => e.Deduction)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("deduction");
            entity.Property(e => e.DeleteFlag)
                .HasDefaultValue(false)
                .HasColumnName("delete_flag");
            entity.Property(e => e.LastUpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("last_updated_date");
            entity.Property(e => e.LastUpdatedId).HasColumnName("last_updated_id");
            entity.Property(e => e.TotalSalary)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("total_salary");

            entity.HasOne(d => d.Account).WithMany(p => p.Salaries)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Salary_Account");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
