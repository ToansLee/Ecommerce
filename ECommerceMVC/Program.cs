using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<FoodChatbotService>();
builder.Services.AddDbContext<ECommerceMVC.Data.FoodOrderingContext>(options => {
	options.UseSqlServer(builder.Configuration.GetConnectionString("FoodOrdering"));
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(10);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/KhachHang/DangNhap";
		options.AccessDeniedPath = "/AccessDenied";
		options.ExpireTimeSpan = TimeSpan.FromDays(30);
	});

// https://docs.automapper.org/en/stable/Dependency-injection.html
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
