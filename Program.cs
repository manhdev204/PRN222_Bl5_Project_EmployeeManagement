using Microsoft.EntityFrameworkCore;
using PRN222_BL5_Project_EmployeeManagement.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Prn222Bl5ProjectEmployeeManagementContext>(
   options => options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn"))
);
builder.Services.AddScoped(typeof(Prn222Bl5ProjectEmployeeManagementContext));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.IdleTimeout = TimeSpan.FromHours(2);
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
