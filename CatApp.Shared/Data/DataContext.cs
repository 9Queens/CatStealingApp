using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatApp.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatApp.Shared.Data
{

    //----------- Required  to run both BEFORE you run the Web App
    // ---------- As Migrations folder should be present in the .Shared project. 
    // ---------- In any other case the solution will not work at all.

    //dotnet ef migrations add InitialCreate --context DataContext --project "C:\Users\konst\source\repos\CatApp\CatApp.Shared\CatApp.Shared.csproj" --startup-project "C:\Users\konst\source\repos\CatApp\CatApp\CatApp.csproj" 
    //dotnet ef database update --project "C:\Users\konst\source\repos\CatApp\CatApp.Shared\CatApp.Shared.csproj" --startup-project "C:\Users\konst\source\repos\CatApp\CatApp\CatApp.csproj"
    //dotnet ef migrations add InitialCreate --context DataContext --project "C:\Users\konst\source\repos\CatApp\CatApp.Shared\CatApp.Shared.csproj" --startup-project "C:\Users\konst\source\repos\CatApp\CatApp\CatApp.csproj"   --output-dir "..\CatApp\Migrations"

    public class DataContext : DbContext
    {

        public DbSet<Cat> Cats { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<CatTag> CatTags { get; set; }

        public DbSet<CatDownloadProgress> CatDownloadProgresses { get; set; }

        //mew options just in case...
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        //with mew builder just in case...
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Tag>()
                 .Property(t => t.Id)
                 .ValueGeneratedOnAdd();

            modelBuilder.Entity<Cat>()
             .Property(c => c.Id)
             .ValueGeneratedOnAdd();


            modelBuilder.Entity<CatTag>()
             .HasKey(ct => new { ct.CatId, ct.TagId });

        }
    }
}
