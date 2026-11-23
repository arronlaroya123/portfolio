
using Npgsql;
using portfolio.Data;
using portfolio.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Database Connection
var connectionString = builder.Configuration
    .GetConnectionString("Portfolio");

builder.Services.AddSingleton<NpgsqlDataSource>(sp =>
    NpgsqlDataSource.Create(connectionString!));

// Register repositories
builder.Services.AddScoped<DatabaseTest>();
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<ProjectImageRepository>();
builder.Services.AddScoped<AccountRepository>();

// Editor Authentication
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Editor/Unlock";
        options.AccessDeniedPath = "/Editor/AccessDenied";
        options.Cookie.Name = "PortfolioEditorMode";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

// Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
