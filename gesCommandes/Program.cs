using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using gesCommandes.Data;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using gesCommandes.Data.Fixtures;
using gesCommandes.Models;

var builder = WebApplication.CreateBuilder(args);

// Connection string for PostgreSQL database
string connectionString = builder.Configuration.GetConnectionString("PostgresConnection")!;

// Adding DbContext to the service container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services for MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// Add session management with a 30-minute timeout
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add authentication services with Cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => 
    {
        options.LoginPath = "/User/Login";
        options.LogoutPath = "/User/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Session expiration time
    });

var app = builder.Build();

// Seed the database if necessary
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    // Uncomment and modify this line if seeding is needed for other models
    Fixtures.Initialize(context); 
    // ProduitFixtures.Initialize(context); 
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Error handling for production
    app.UseHsts(); // Enforce HTTPS in production
}

app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
app.UseStaticFiles(); // Serve static files

app.UseRouting(); // Enable routing
app.UseSession(); // Enable session handling

app.UseAuthentication(); // Enable authentication
app.UseAuthorization(); // Enable authorization

app.UseStatusCodePagesWithReExecute("/Home/NotFound", "?statusCode={0}");

// Define routing for MVC controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();
