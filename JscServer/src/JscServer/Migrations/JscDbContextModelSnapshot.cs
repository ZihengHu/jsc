using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using JscServer.Models;

namespace JscServer.Migrations
{
    [DbContext(typeof(JscDbContext))]
    partial class JscDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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
