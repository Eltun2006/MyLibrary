using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using MyLibrary.DAL;
using MyLibrary.Repository;
using MyLibrary.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ İlk öncə DATABASE_URL yoxla, sonra DefaultConnection
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// ✅ Əgər DATABASE_URL varsa, postgresql:// formatına çevir
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgres://"))
{
    connectionString = connectionString.Replace("postgres://", "postgresql://");
}

builder.Services.AddDbContext<ApDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"/app/dataprotection-keys"))
    .SetApplicationName("MyLibraryApp");


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