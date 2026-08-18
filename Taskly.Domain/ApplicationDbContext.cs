using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Domain.Entities;

namespace Taskly.Domain
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        // Design-time constructor for migrations
        public ApplicationDbContext() : base(DesignTimeDbContextOptions())
        {
        }

        private static DbContextOptions<ApplicationDbContext> DesignTimeDbContextOptions()
        {
            // Build the path to the PetGroomer.Api project
            var webProjectPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Taskly.Api");

            // Load the configuration from appsettings.json in the .Api project
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(webProjectPath) // Set the base path to the .Api directory
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Load appsettings.json
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Choose the appropriate database provider here
            builder.UseSqlite(connectionString); // Or UseSqlServer, UseNpgsql, etc.

            return builder.Options;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Leads> Leads { get; set; }

        public DbSet<CookieFiles> CookieFiles { get; set; }

        public DbSet<ExternalLinks> ExternalLinks { get; set; }

        public DbSet<Icebreakers> Icebreakers { get; set; }

        public DbSet<CustomMessages> CustomMessages { get; set; }

        public DbSet<Templates> Templates { get; set; }

        public DbSet<GoogleAI> GoogleAIs { get; set; }

        public DbSet<LMStudio> LMStudios { get; set; }

        public DbSet<Settings> Settings { get; set; }

        public DbSet<Campaigns> Campaigns { get; set; }

        public DbSet<CampaignSequences> CampaignSequences { get; set; }

        public DbSet<CampaignMessages> CampaignMessages { get; set; }

        public DbSet<CampaignContent> CampaignContents { get; set; }    
    }
}
