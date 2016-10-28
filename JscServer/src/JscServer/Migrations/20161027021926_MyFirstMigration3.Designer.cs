using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using JscServer.Models;

namespace JscServer.Migrations
{
    [DbContext(typeof(JscDbContext))]
    [Migration("20161027021926_MyFirstMigration3")]
    partial class MyFirstMigration3
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.1");

            modelBuilder.Entity("JscServer.Models.Coverage", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Data");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("Coverages");
                });
        }
    }
}
