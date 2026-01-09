using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
// DbContext
builder.Services.AddDbContext<Infrastructure.Data.SMSDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.Configure<SendGridSettings>(
    builder.Configuration.GetSection("SendGrid"));
builder.Services.AddTransient<IEmailService, SendGridEmailService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";  // redirect if not logged in
        options.LogoutPath = "/Auth/Logout";
    });



var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SMSDbContext>();

    if (!context.Bandas.Any(u => u.BanRole == "admin"))
    {
        var admin = new Banda
        {
            Ban_username = "admin@school.com",
            BanPassword = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            BanActive = true,
            BanRole = "admin"
        };
        context.Bandas.Add(admin);
        context.SaveChanges();
    }
}

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
app.UseAuthentication();
app.UseAuthorization();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Auth}/{action=Login}/{id?}");
app.MapGet("/", context =>
{
    context.Response.Redirect("/Auth/Auth/Login");
    return Task.CompletedTask;
});

app.Run();
