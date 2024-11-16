﻿// <auto-generated />
using System;
using Database.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Database.Client.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20241114185136_AddPortsTable")]
    partial class AddPortsTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("Database.Client.DbEntities.CustomPortsDbEntity", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<string>("PathToExe")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("path");

                    b.Property<byte>("PortEnum")
                        .HasColumnType("INTEGER")
                        .HasColumnName("port");

                    b.HasKey("Name");

                    b.ToTable("custom_ports", "main");
                });

            modelBuilder.Entity("Database.Client.DbEntities.DisabledDbEntity", b =>
                {
                    b.Property<string>("AddonId")
                        .HasColumnType("TEXT")
                        .HasColumnName("addon_id");

                    b.HasKey("AddonId");

                    b.ToTable("disabled_addons", "main");
                });

            modelBuilder.Entity("Database.Client.DbEntities.GamePathsDbEntity", b =>
                {
                    b.Property<string>("Game")
                        .HasColumnType("TEXT")
                        .HasColumnName("game");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT")
                        .HasColumnName("path");

                    b.HasKey("Game");

                    b.ToTable("game_paths", "main");
                });

            modelBuilder.Entity("Database.Client.DbEntities.PlaytimesDbEntity", b =>
                {
                    b.Property<string>("AddonId")
                        .HasColumnType("TEXT")
                        .HasColumnName("addon_id");

                    b.Property<TimeSpan>("Playtime")
                        .HasColumnType("TEXT")
                        .HasColumnName("playtime");

                    b.HasKey("AddonId");

                    b.ToTable("playtimes", "main");
                });

            modelBuilder.Entity("Database.Client.DbEntities.RatingDbEntity", b =>
                {
                    b.Property<string>("AddonId")
                        .HasColumnType("TEXT")
                        .HasColumnName("addon_id");

                    b.Property<byte>("Rating")
                        .HasColumnType("INTEGER")
                        .HasColumnName("rating");

                    b.HasKey("AddonId");

                    b.ToTable("rating", "main");
                });

            modelBuilder.Entity("Database.Client.DbEntities.SettingsDbEntity", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("value");

                    b.HasKey("Name");

                    b.ToTable("settings", "main");
                });
#pragma warning restore 612, 618
        }
    }
}
