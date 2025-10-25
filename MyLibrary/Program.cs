using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using MyLibrary.DAL;
using MyLibrary.Services;
using MyLibrary.Repository;

var builder = WebApplication.CreateBuilder(args);

// ✅ UseSqlServer istifadə et (UseNpgsql DEYİL!)
builder.Services.AddDbContext<ApDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));

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
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<UserRepo>();

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