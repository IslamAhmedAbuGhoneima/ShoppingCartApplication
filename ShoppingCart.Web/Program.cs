using Microsoft.EntityFrameworkCore;
using ShoppingCart.DataAccess.Data;
using ShoppingCart.DataAccess.implementation;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ShoppingCart.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();


            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });

            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();


            builder.Services.AddSingleton<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IGenericRepository<Category>, CategoryRepository>();
            builder.Services.AddScoped<IGenericRepository<Product>, ProductRepository>();
            builder.Services.AddScoped<IGenericRepository<Order>, OrderRepository>();
            builder.Services.AddScoped<IGenericRepository<OrderItem>, OrderItmeRepository>();


            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = builder.Configuration.GetConnectionString("conStr")!;
                options.UseSqlServer(connectionString);
            });


            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Password.RequireUppercase = false;
            }).AddDefaultUI()
              .AddDefaultTokenProviders()
              .AddEntityFrameworkStores<AppDbContext>();

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

            app.UseSession();

            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();


            app.MapControllerRoute(
                name: "Area",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/");

            app.MapControllerRoute(
                name: "Area",
                pattern: "{area=Admin}/{controller=Users}/{action=Index}/");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
