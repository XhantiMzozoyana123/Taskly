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
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Taskly;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Leads> Leads { get; set; }

        public DbSet<SocialLogins> SocialLogins { get; set; }

        public DbSet<ExternalLinks> ExternalLinks { get; set; }

        public DbSet<Icebreakers> Icebreakers { get; set; }

        public DbSet<Campaigns> Campaigns { get; set; }

        public DbSet<Messages> Messages { get; set; }
    }
}
