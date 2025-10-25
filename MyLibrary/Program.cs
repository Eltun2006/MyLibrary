using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using MyLibrary.DAL;
using MyLibrary.Services;
using MyLibrary.Repository;  // ← BU SƏTRİ ƏLAVƏ ET

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600;
});

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<UserRepo>();  // ← BU SƏTRİ ƏLAVƏ ET

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Account}/{id?}");

app.Run();