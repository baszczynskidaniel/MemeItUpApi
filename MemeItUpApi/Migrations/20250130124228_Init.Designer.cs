﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MemeItUpApi.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250130124228_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("MemeTemplate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("MemeTemplates");
                });

            modelBuilder.Entity("TextPosition", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<float>("Bottom")
                        .HasColumnType("REAL");

                    b.Property<float>("Left")
                        .HasColumnType("REAL");

                    b.Property<Guid?>("MemeTemplateId")
                        .HasColumnType("TEXT");

                    b.Property<float>("Right")
                        .HasColumnType("REAL");

                    b.Property<float>("Top")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("MemeTemplateId");

                    b.ToTable("TextPositions");
                });

            modelBuilder.Entity("TextPosition", b =>
                {
                    b.HasOne("MemeTemplate", "MemeTemplate")
                        .WithMany("TextPositions")
                        .HasForeignKey("MemeTemplateId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("MemeTemplate");
                });

            modelBuilder.Entity("MemeTemplate", b =>
                {
                    b.Navigation("TextPositions");
                });
#pragma warning restore 612, 618
        }
    }
}
