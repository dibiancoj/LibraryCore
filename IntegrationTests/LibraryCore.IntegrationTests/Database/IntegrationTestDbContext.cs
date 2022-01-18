using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using LibraryCore.IntegrationTests.DatabaseModels;
using Microsoft.EntityFrameworkCore.Design;

namespace LibraryCore.IntegrationTests.Database
{
    public partial class IntegrationTestDbContext : DbContext
    {
        public IntegrationTestDbContext()
        {
        }

        public IntegrationTestDbContext(DbContextOptions<IntegrationTestDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<State> States { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=localhost,5434;Initial Catalog=IntegrationTest;User Id=sa;Password=Pass@word;trustServerCertificate=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<State>(entity =>
            {
                entity.HasKey(e => new { e.TestId, e.StateId })
                    .HasName("PK_Table_1");

                entity.Property(e => e.StateId).ValueGeneratedOnAdd();

                entity.Property(e => e.Description)
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
