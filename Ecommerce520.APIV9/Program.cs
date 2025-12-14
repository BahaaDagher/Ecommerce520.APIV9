using Ecommerce520.APIV9;
using Ecommerce520.APIV9.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Stripe;
using System.Globalization;
using System.Text;
namespace Ecommerce520.APIV9520.APIV9520.APIV9
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy.WithOrigins("http://127.0.0.1:5500")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                    });
            });

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddControllersWithViews();

            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string"
                    + "'DefaultConnection' not found.");

            //AppConfiguration.Config(builder.Services, connectionString);

            builder.Services.Config(connectionString);
            builder.Services.RegisterMapsterConfig();

            var JwtSettings = builder.Configuration.GetSection("JwtSettings");

            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = JwtSettings["validIssuer"],
                    ValidAudience = JwtSettings["validAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings["securityKey"]))
                };
            });

            builder.Services.AddSignalR();
            //builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            const string defaultCulture = "en";
            var supportedCultures = new[]
            {
                new CultureInfo(defaultCulture),
                new CultureInfo("es"),
                new CultureInfo("ar")
            };
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            var app = builder.Build();

            //using (var scope = app.Services.CreateScope())
            //{
            //    var dbInitializr = scope.ServiceProvider.GetRequiredService<IDBInitializr>();
            //    dbInitializr.Initialize();
            //}

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapScalarApiReference();
                app.MapOpenApi();
            }
            app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);


            app.UseCors();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<RealTimeHub>("/realtimehub");
            app.MapControllers();

            app.Run();
        }
    }
}
