using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using StatisticEI.Services;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<InvStatisticService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
