using EMart.Data;
using EMart.Repositories.Implementation;
using EMart.Repositories.Interfaces;
using EMart.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

// Load environment variables from .env
DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// Add services to the container
// =====================================================

builder.Services.AddControllersWithViews();


// =====================================================
// Database
// =====================================================

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DBCS")));

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IMcpClientService, McpClientService>();
builder.Services.AddHttpClient<IAiAssistantService, AiAssistantService>();
// =====================================================
// Repositories
// =====================================================

builder.Services.AddScoped<ICategoriesRepository,
    CategoriesRepository>();

builder.Services.AddScoped<ISubCategoriesRepository,
    SubCategoriesRepository>();

builder.Services.AddScoped<IBrandsRepository,
    BrandsRepository>();

builder.Services.AddScoped<IRolesRepository,
    RolesRepository>();

builder.Services.AddScoped<IUsersRepository,
    UsersRepository>();

builder.Services.AddScoped<IUnitsRepository,
    UnitsRepository>();

builder.Services.AddScoped<IProductsRepository,
    ProductsRepository>();

builder.Services.AddScoped<ICartRepository,
    CartRepository>();

builder.Services.AddScoped<IAddressRepository,
    AddressRepository>();

builder.Services.AddScoped<IAccountRepository,
    AccountRepository>();
builder.Services.AddScoped<IOrderRepository,
    OrderRepository>();
builder.Services.AddScoped<IReportRepository,
    ReportRepository>();
builder.Services.AddScoped<ICustomers,
    CustomersRepository>();
builder.Services.AddScoped<IPayments,
    PaymentsRepository>();
builder.Services.AddScoped<IWishlistRepository,
    WishlistRepository>();
builder.Services.AddScoped<ICouponRepository,
    CouponRepository>();
builder.Services.AddScoped<IReviewRepository,
    ReviewRepository>();
builder.Services.AddScoped<IBeeCoinsRepository,
    BeeCoinsRepository>();
builder.Services.AddScoped<IReorderRepository,
    ReorderRepository>();
builder.Services.AddScoped<IDeliveryRepository,
    DeliveryRepository>();
builder.Services.AddScoped<IVendorRepository,
    VendorRepository>();
builder.Services.AddSingleton<IPincodeService,
    PincodeService>();



// =====================================================
// Cookie Authentication -- Admin, Customer, Vendor, DeliveryPartners
// =====================================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            "CustomerCookie";

        options.DefaultSignInScheme =
            "CustomerCookie";

        options.DefaultChallengeScheme =
            "CustomerCookie";
    })
    // Customer Cookie
    .AddCookie("CustomerCookie", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(30);

        options.SlidingExpiration = true;

        options.Cookie.Name = "EMart.Customer";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    })

    // Admin Cookie
    .AddCookie("AdminCookie", options =>
    {
        options.LoginPath = "/Admin/Account/Login";
        options.AccessDeniedPath = "/Admin/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(30);

        options.SlidingExpiration = true;

        options.Cookie.Name = "EMart.Admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    })

    // Delivery Partner Cookie
    .AddCookie("DeliveryCookie", options =>
    {
        options.LoginPath = "/Delivery/Account/Login";
        options.AccessDeniedPath = "/Delivery/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);

        options.SlidingExpiration = true;

        options.Cookie.Name = "Buybee.Delivery";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    })

    // Vendor Cookie
    .AddCookie("VendorCookie", options =>
    {
        options.LoginPath = "/Vendor/Account/Login";
        options.AccessDeniedPath = "/Vendor/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);

        options.SlidingExpiration = true;

        options.Cookie.Name = "Buybee.Vendor";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });


// =====================================================
// Session
// =====================================================

builder.Services.AddSession(options =>
{
    options.IdleTimeout =
        TimeSpan.FromMinutes(30);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;
});


var app = builder.Build();


// =====================================================
// HTTP Request Pipeline
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/NotFound");
app.UseHttpsRedirection();

app.UseStaticFiles();


// Routing should come before Session/Auth
app.UseRouting();


// Session
app.UseSession();


// Authentication
app.UseAuthentication();


// Authorization
app.UseAuthorization();


// =====================================================
// Static Assets
// =====================================================

app.MapStaticAssets();


// =====================================================
// Areas Route
// =====================================================

// Admin Orders URL convenience redirect (without interfering with customer Order link generation)
app.MapGet("/Admin/Orders", () => Results.Redirect("/Admin/Order"));
app.MapGet("/Admin/Orders/{action}", (string action) => Results.Redirect($"/Admin/Order/{action}"));

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");


// =====================================================
// Default Route
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();