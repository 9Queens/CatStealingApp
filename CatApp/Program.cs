using CatApp.HostedServices;
using CatApp.Services;
using CatApp.Services.Services;
using CatApp.Shared.Data;
using CatApp.Shared.Options;
using Hangfire;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContextFactory<DataContext>(options =>
        {
            if (builder.Environment.IsEnvironment("Testing"))
            {
                //we have installed here in the CatApp --> dotnet add package Microsoft.EntityFrameworkCore.InMemory
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
            }
            else
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.CommandTimeout(180)
                );
            }
        });

        // Register Hosted Service for Database Migrations
        builder.Services.AddHostedService<DatabaseMigrationService>();


        Console.WriteLine($"------------------------ TEST ----------------------------Connection string: {builder.Configuration.GetConnectionString("DefaultConnection")}");


        // Add Hangfire services
        builder.Services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        builder.Services.AddHangfireServer(); // Required to run Hangfire jobs


        //HttpClient Configuration for Cat API
        builder.Services.Configure<HttpClientOptions>(builder.Configuration.GetSection("HttpClientOptions"));
        builder.Services.AddHttpClient("CatApi");
        builder.Services.AddHostedService<CatApiHttpClientHostedService>();  // Mew downloader

        //Configure options from appsettings.json
        builder.Services.Configure<MewServiceOptions>(builder.Configuration.GetSection("CatAppServices:MewService"));
        builder.Services.Configure<CatServiceOptions>(builder.Configuration.GetSection("CatAppServices:CatService"));

        //Register services
        builder.Services.AddScoped<ITagService, TagService>();
        builder.Services.AddScoped<ICatService, CatService>();
        builder.Services.AddScoped<IMewService, MewService>();  // Mew downloader
        builder.Services.AddScoped<ICatDownloadProgressService, CatDownloadProgressService>();  // Mew download tracker
        builder.Services.AddScoped<CatDownloadProgressService>();


        //because i dont want to  expose internal ids of my database (i keep it null on fetch ) and it gets an incremental so i did the following
        //thoug i have image of cats set to null  -INTENTIONALLY- as i want fast fetches for the demo !!!!
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = null;
                //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                //opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            });

        var app = builder.Build();


        //this didn't work so ... we will use scope
        //GlobalJobFilters.Filters.Add(app.Services.GetRequiredService<CatDownloadProgressService>());

        using (var scope = app.Services.CreateScope())
        {
            var filter = scope.ServiceProvider.GetRequiredService<CatDownloadProgressService>();
            GlobalJobFilters.Filters.Add(filter);
        }

        //Differentiating the environment as in tests i use in memory database NOT physical one
        //I do the same also in the IHostedService  --> DatabaseMigrationService  
        if (String.Equals(app.Environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>().CreateDbContext();
                db.Database.EnsureCreated(); // <---- THIS --->For in-memory database ----> FOR TESTS
                // db.Database.Migrate(); // <-------- NOT THIS ( as this requires relational database --> FOR NOT TESTS !!!
            }
        }
        else
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>().CreateDbContext();
                db.Database.EnsureCreated();
            }

            Console.WriteLine($"------------------------ TEST ----------------------------Connection string: {builder.Configuration.GetConnectionString("DefaultConnection")}");

            // Retry logic for database connection ( this helps me in docker mainly )
            var configuration = app.Configuration;
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var maxRetries = 10;
            var delay = TimeSpan.FromSeconds(5);

            // Mainly for docker compose ... to give some time for sql server to be availiable
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using var connection = new SqlConnection(connectionString);
                    connection.Open();
                    Console.WriteLine("Successfully connected to SQL Server.");
                    break;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Attempt {i + 1} failed: {ex.Message}");
                    if (i == maxRetries - 1) throw;

                    Task.Delay(delay); // Wait before retrying
                }
            }
        }



        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        //Because dockerized solution mew us regarding CORS 
        app.UseCors("AllowAll");

        app.UseAuthorization();

        //Hangfire Dashboard for monitoring jobs
        app.UseHangfireDashboard();
        app.MapControllers();
        app.Run();
    }
}