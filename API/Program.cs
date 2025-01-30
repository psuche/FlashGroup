
using API.Repository;
using Microsoft.Data.SqlClient;
using System.Data;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            //builder.Services.AddOpenApi();
            //builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo
            //    {
            //        Title = "My API",
            //        Version = "v1",
            //        Description = "An example API using Swagger in .NET Core",
            //    });
            //});


            builder.Services.AddControllers();
            builder.Services.AddTransient<IDbConnection>(sp => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<ISensitiveWordsRepository, SensitiveWordsRepository>();
            builder.Services.AddMemoryCache();
            //builder.Services.AddResponseCompression(); //not familiar enough with this library yet
            //but I know that compression of any kind will help lower traffic on the wire provided the implementation is sound.

            //Loggeeeng
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            //Would need to read up some more on implementation practices regarding ExceptionMiddleware and configure it here.
            //Then use it in combination with whatever logging provider is chosen for the project.

            //Config Management too? barely any experience configuring it from scratch, another thing I would research before making a call on it.
            //Rate limiting, AspNetCoreRateLimit makes this easy, would just have to read some documentation on configuring from scratch.
            //Quick google search, found this: https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-9.0

            //Authentication, JWT would be nice and simple

            builder.Services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                //app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "sw_Sensitive Words API");

                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
