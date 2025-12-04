

using Ecommerce520.APIV9.JwtFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce520.APIV9
{
    public static class AppConfiguration
    {

        public static void Config(this IServiceCollection services , string connectionString )
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                //options.UseSqlServer(builder.Configuration.GetSection("ConnectionStrings")["DefaultConnection"]); 
                //options.UseSqlServer(builder.Configuration["ConnectionStrings : DefaultConnection"]); 
                //options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")); 
                options.UseSqlServer(connectionString);
            });
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                //options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false; 
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IRepository<Category>, Repository<Category>>();
            //services.AddSingleton<IRepository<Category>, Repository<Category>>();
            //services.AddTransient<IRepository<Category>, Repository<Category>>();
            services.AddScoped<IRepository<Brand>, Repository<Brand>>();
            services.AddScoped<IRepository<ProductSubImage>, Repository<ProductSubImage>>();
            services.AddScoped<IRepository<ProductColor>, Repository<ProductColor>>();
            services.AddScoped<IRepository<Cart>, Repository<Cart>>();
            services.AddScoped<IRepository<Promotion>, Repository<Promotion>>();
            services.AddScoped<IRepository<ApplicationUserOTP>, Repository<ApplicationUserOTP>>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IJwtHandler, JwtHandler>();
            services.AddScoped<IDBInitializr, DBInitializr>();
        }
    }
}
