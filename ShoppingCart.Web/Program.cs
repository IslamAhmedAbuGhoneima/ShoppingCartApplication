using Microsoft.EntityFrameworkCore;
using ShoppingCart.DataAccess.Data;
using ShoppingCart.DataAccess.implementation;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ShoppingCart.Web
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });

            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();


            builder.Services.AddScoped<IGenericRepository<Category>, CategoryRepository>();
            builder.Services.AddScoped<IGenericRepository<Entities.Models.Product>, ProductRepository>();
            builder.Services.AddScoped<IGenericRepository<Entities.Models.Coupon>, CouponRepository>();
            builder.Services.AddScoped<IGenericRepository<Order>, OrderRepository>();
            builder.Services.AddScoped<IGenericRepository<OrderItem>, OrderItmeRepository>();


            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = configuration.GetConnectionString("conStr")!;
                options.UseSqlServer(connectionString);
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Password.RequireUppercase = false;
            }).AddDefaultUI()
              .AddDefaultTokenProviders()
              .AddEntityFrameworkStores<AppDbContext>();


            #region Google & Facebook Authentication

            builder.Services.AddAuthentication()
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            })
            .AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = configuration["Authentication:Facebook:AppSecret"];
            }).AddOAuth("GitHub", options =>
            {
                options.ClientId = configuration["Authentication:GitHub:ClientId"];
                options.ClientSecret = configuration["Authentication:GitHub:ClientSecret"];

                options.CallbackPath = new PathString("/signin-github");

                // GitHub endpoints for OAuth
                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                options.UserInformationEndpoint = "https://api.github.com/user";

                options.SaveTokens = true;

                options.Scope.Add("user:email");

                // Configure user data retrieval
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

                // OAuth event configuration
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        try
                        {
                            // Helper function to get response from GitHub API
                            async Task<JsonDocument> GetGitHubDataAsync(string endpoint, string accessToken)
                            {
                                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                                var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                                response.EnsureSuccessStatusCode();

                                return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                            }

                            // Retrieve user information
                            var user = await GetGitHubDataAsync(context.Options.UserInformationEndpoint, context.AccessToken);
                            context.RunClaimActions(user.RootElement);

                            // Retrieve user's email if it is private
                            var emails = await GetGitHubDataAsync("https://api.github.com/user/emails", context.AccessToken);
                            var primaryEmail = emails.RootElement
                                .EnumerateArray()
                                .FirstOrDefault(email => email.GetProperty("primary").GetBoolean())
                                .GetProperty("email")
                                .GetString();

                            if (!string.IsNullOrEmpty(primaryEmail))
                            {
                                context.Identity?.AddClaim(new Claim(ClaimTypes.Email, primaryEmail));
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            // Handle HTTP request errors
                            context.Fail($"Error retrieving GitHub user information: {ex.Message}");
                        }
                        catch (JsonException ex)
                        {
                            // Handle JSON parsing errors
                            context.Fail($"Error parsing GitHub user data: {ex.Message}");
                        }
                    }
                };
            });

            #endregion


            StripeConfiguration.ApiKey = configuration["Stripe:Secretkey"];

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
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "Area",
                pattern: "{area=Admin}/{controller=Users}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
