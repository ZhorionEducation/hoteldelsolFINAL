using Hotel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.CookiePolicy;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(Program));
    });

builder.Services.AddDbContext<HotelContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("conexion")));

// Agregar IWebHostEnvironment al contenedor de servicios
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.Always;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.OnAppendCookie = cookieContext => 
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext => 
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});

void CheckSameSite(HttpContext httpContext, CookieOptions options)
{
    if (options.SameSite == SameSiteMode.None)
    {
        options.SameSite = SameSiteMode.Lax;
    }
}

// Configure the authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Usuarios/Login";
    options.LogoutPath = "/Usuarios/Logout";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax; // Cambiado a Lax
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.SaveTokens = true;
    
    // Ensure callback path matches your route exactly
    options.CallbackPath = "/signin-google";
    
    // Configure state validation and correlation cookie
    options.CorrelationCookie = new CookieBuilder
    {
        SameSite = SameSiteMode.Lax,
        SecurePolicy = CookieSecurePolicy.Always,
        HttpOnly = true,
        IsEssential = true
    };
});

// Configurar la localización
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("es-ES"),
        new CultureInfo("en-US")
    };
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("es-ES");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Agregar servicios de sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5264);
    
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();

    app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Agregar UseAuthentication antes de UseAuthorization
app.UseSession();
app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();