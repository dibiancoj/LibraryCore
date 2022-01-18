﻿// <auto-generated />
using System;
using LibraryCore.IntegrationTests.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace LibraryCore.IntegrationTests.Migrations
{
    [DbContext(typeof(IntegrationTestDbContext))]
    [Migration("20220117173035_insert-states-sp")]
    partial class InsertStatesSp
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("LibraryCore.IntegrationTests.DatabaseModels.State", b =>
                {
                    b.Property<Guid>("TestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("StateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StateId"), 1L, 1);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(25)
                        .IsUnicode(false)
                        .HasColumnType("varchar(25)");

                    b.HasKey("TestId", "StateId")
                        .HasName("PK_Table_1");

                    b.ToTable("States");
                });
#pragma warning restore 612, 618
        }
    }
}
