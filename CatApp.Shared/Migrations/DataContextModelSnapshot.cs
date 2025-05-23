﻿// <auto-generated />
using System;
using CatApp.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CatApp.Shared.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CatApp.Shared.Entities.Cat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ApiId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("CreatedOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("Height")
                        .HasColumnType("int");

                    b.Property<byte[]>("Image")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("ImageHash")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Width")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Cats");
                });

            modelBuilder.Entity("CatApp.Shared.Entities.CatDownloadProgress", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("BatchFailures")
                        .HasColumnType("int");

                    b.Property<int>("CatsDownloaded")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("CompletedOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("DoublicatesOccured")
                        .HasColumnType("int");

                    b.Property<int>("ErrorsOccured")
                        .HasColumnType("int");

                    b.Property<string>("Messages")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("StartedOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<int>("TotalCats")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("CatDownloadProgresses");
                });

            modelBuilder.Entity("CatApp.Shared.Entities.CatTag", b =>
                {
                    b.Property<int>("CatId")
                        .HasColumnType("int");

                    b.Property<int>("TagId")
                        .HasColumnType("int");

                    b.HasKey("CatId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("CatTags");
                });

            modelBuilder.Entity("CatApp.Shared.Entities.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("CatApp.Shared.Entities.CatTag", b =>
                {
                    b.HasOne("CatApp.Shared.Entities.Cat", "Cat")
                        .WithMany("CatTags")
                        .HasForeignKey("CatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CatApp.Shared.Entities.Tag", "Tag")
                        .WithMany("CatTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cat");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("CatApp.Shared.Entities.Cat", b =>
                {
                    b.Navigation("CatTags");
                });

            modelBuilder.Entity("CatApp.Shared.Entities.Tag", b =>
                {
                    b.Navigation("CatTags");
                });
#pragma warning restore 612, 618
        }
    }
}
