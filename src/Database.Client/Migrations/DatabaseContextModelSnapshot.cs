﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Database.Client.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    sealed partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.6");

            modelBuilder.Entity("Common.Client.Config.DbEntities.DisabledDbEntity", b =>
                {
                    b.Property<string>("AddonId")
                        .HasColumnType("TEXT")
                        .HasColumnName("addon_id");

                    b.HasKey("AddonId");

                    b.ToTable("disabled_addons", "main");
                });

            modelBuilder.Entity("Common.Client.Config.DbEntities.GamePathsDbEntity", b =>
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

            modelBuilder.Entity("Common.Client.Config.DbEntities.PlaytimesDbEntity", b =>
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

            modelBuilder.Entity("Common.Client.Config.DbEntities.RatingDbEntity", b =>
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

            modelBuilder.Entity("Common.Client.Config.DbEntities.SettingsDbEntity", b =>
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
