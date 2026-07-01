using Microsoft.EntityFrameworkCore;
using PublicadorDeAtas.Context;
using Microsoft.AspNetCore.Identity;
using PublicadorDeAtas.Models;
using PublicadorARP.Services.Interfaces;
using PublicadorARP.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// CORREÇÃO: Certifique-se de que o pacote NuGet Npgsql esteja instalado
builder.Services.AddDbContext<AppDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseNpgsql(connectionString);
    });

builder.Services.AddSingleton(builder.Configuration);

builder.Services.AddHttpClient<IPNCPService, PNCPService>(client =>
{
    var route = builder.Configuration["ApiPNCP:Route"];
    if (!string.IsNullOrEmpty(route))
    {
        client.BaseAddress = new Uri(route);
    }
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// CORREÇÃO: Ordem correta do Pipeline de Middleware para o Identity
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();