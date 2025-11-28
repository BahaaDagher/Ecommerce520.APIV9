using Ecommerce520.APIV9;
using Ecommerce520.APIV9.Configurations;
using Scalar;
using Scalar.AspNetCore;
using Ecommerce520.APIV9.DTOs.Response;
namespace Ecommerce520.APIV9520.APIV9520.APIV9
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
