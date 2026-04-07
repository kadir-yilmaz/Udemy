using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Udemy.WebUI.Extensions;
using Udemy.WebUI.Handler;
using Udemy.WebUI.Helpers;
using Udemy.WebUI.Services.Abstract;
using Udemy.WebUI.Services.Concrete;
using Udemy.WebUI.Settings;
using Udemy.WebUI.Validators;

var builder = WebApplication.CreateBuilder(args);

// Options Pattern - appsettings.json'dan ayarları okuma
builder.Services.Configure<ServiceApiSettings>(builder.Configuration.GetSection("ServiceApiSettings"));
builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection("ClientSettings"));

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Helpers
builder.Services.AddSingleton<PhotoHelper>();



// Handlers
builder.Services.AddScoped<ResourceOwnerPasswordTokenHandler>();
builder.Services.AddScoped<ClientCredentialTokenHandler>();

// HttpClient Services
builder.Services.AddHttpClientServices(builder.Configuration);

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opts =>
    {
        opts.LoginPath = "/Auth/SignIn";
        opts.ExpireTimeSpan = TimeSpan.FromDays(30);
        opts.SlidingExpiration = true;
        opts.Cookie.Name = "UdemyCookie";
    });

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CourseCreateInputValidator>();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
