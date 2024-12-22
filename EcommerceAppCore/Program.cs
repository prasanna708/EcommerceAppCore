using EcommerceAppCore.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<EcommerceDbContext>();
builder.Services.AddHttpClient();   //
//builder.Services.AddTransient<ApiService>();  //Adding API Service
builder.Services.AddDistributedMemoryCache();   //Add in-memory cache for sessions
builder.Services.AddSession(options =>          
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);     //set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();  //

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();   //Enable session middleware

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Login}/{id?}");

app.Run();
