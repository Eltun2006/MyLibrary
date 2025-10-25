using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using MyLibrary.DAL;
using MyLibrary.Repository;
using MyLibrary.Services;
using System;


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgres://"))
{
    // Render formatından Npgsql formatına çevir
    var uri = new Uri(connectionString);
    var properConnectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";

    builder.Configuration["ConnectionStrings:DefaultConnection"] = properConnectionString;
    connectionString = properConnectionString;
}



// ✅ UseSqlServer istifadə et (UseNpgsql DEYİL!)
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
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<UserRepo>();




var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApDbContext>();
        context.Database.Migrate();
        Console.WriteLine("Migration uğurla tamamlandı!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Migration xətası: {Message}", ex.Message);
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Account}/{id?}");

app.Run();