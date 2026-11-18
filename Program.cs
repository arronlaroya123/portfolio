using Npgsql;
using portfolio.Data;
using portfolio.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database Connection
var connectionString = builder.Configuration
    .GetConnectionString("Portfolio");

builder.Services.AddSingleton<NpgsqlDataSource>(sp =>
    NpgsqlDataSource.Create(connectionString!));

// Register services
builder.Services.AddScoped<DatabaseTest>();
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<ProjectImageRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();