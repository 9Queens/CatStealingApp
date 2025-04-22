using System;
using CatApp.Shared.Data;
using CatApp.Shared.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CatApp.HostedServices
{
    public class DatabaseMigrationService : IHostedService
    {
         private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<DatabaseMigrationService> _logger;
        private readonly IHostEnvironment _environment; // cause i need to differentiate again if test or not

        public DatabaseMigrationService( IServiceProvider serviceProvider, ILogger<DatabaseMigrationService> logger, IHostEnvironment environment)
        {
            _serviceProvider = serviceProvider;
            _environment = environment; // Initialize environment
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                try
                {
                    if (String.Equals(_environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
                    {
                        await dbContext.Database.EnsureCreatedAsync(cancellationToken); // For in-memory database
                    }
                   // else
                   // {
                   //     //i removed this as its getting created outside
                   //     //await dbContext.Database.MigrateAsync(cancellationToken); // For relational database
                   // }
                    _logger.LogInformation("Database setup completed successfully.");

                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while migrating the database: {ex.Message}");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // This method can be used to clean up resources when the app is shutting down (if needed)
            return Task.CompletedTask;
        }
    }
}

