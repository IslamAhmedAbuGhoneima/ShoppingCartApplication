using Microsoft.EntityFrameworkCore;
using ShoppingCart.DataAccess.Data;
using ShoppingCart.DataAccess.implementation;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.Repositories;

namespace ShoppingCart.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();


            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();


            builder.Services.AddScoped<IGenericRepository<Category>, CategoryRepository>();
            builder.Services.AddScoped<IGenericRepository<Product>, ProductRepository>();


            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = builder.Configuration.GetConnectionString("conStr")!;
                options.UseSqlServer(connectionString);
            });

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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Admin}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
