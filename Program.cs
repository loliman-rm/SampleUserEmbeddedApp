using SampleUserEmbeddedApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient(); // Register IHttpClientFactory
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None; // Allow cross-site iframes
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Required for SameSite=None
});

// Add services to the container.
builder.Services.Configure<AppSettings>
    (builder.Configuration.GetSection("AppSettings"));
builder.Services.AddTransient<UserInfo>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // Enable session
app.UseAuthorization();

// Security headers to allow iframe embedding
app.Use(async (context, next) =>
{
    context.Response.Headers.Remove("X-Frame-Options"); // Remove frame blocking
    context.Response.Headers["Content-Security-Policy"] = "frame-ancestors 'self' http://localhost:64762/";
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{search?}");

app.MapControllerRoute(
    name: "Timezone",
    pattern: "{controller=Timezone}/{action=Index}/");

app.Run();
