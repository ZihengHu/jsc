using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using JscServer.Models;

namespace JscServer.Migrations
{
    [DbContext(typeof(JscDbContext))]
    [Migration("20161107060724_MyFirstMigration")]
    partial class MyFirstMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.1");

            modelBuilder.Entity("JscServer.Models.Coverage", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Data");

                    b.Property<DateTime>("InsertTime")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("UpdateTime")
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("Coverages");
                });

            modelBuilder.Entity("JscServer.Models.Injection", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("InjectedPath");

                    b.Property<DateTime>("InsertTime")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("OriginalUrl");

                    b.Property<DateTime>("UpdateTime")
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.ToTable("Injections");
                });
        }
    }
}
