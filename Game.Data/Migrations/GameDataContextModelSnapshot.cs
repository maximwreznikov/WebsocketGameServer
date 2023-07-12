﻿// <auto-generated />
using System;
using Game.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Game.Data.Migrations
{
    [DbContext(typeof(GameDataContext))]
    partial class GameDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.8");

            modelBuilder.Entity("Game.Data.Player", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<string>("Udid")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("udid");

                    b.HasKey("Id")
                        .HasName("pk_players");

                    b.HasIndex("Udid")
                        .IsUnique()
                        .HasDatabaseName("ix_players_udid");

                    b.ToTable("players", (string)null);
                });

            modelBuilder.Entity("Game.Data.Resource", b =>
                {
                    b.Property<Guid>("PlayerId")
                        .HasColumnType("TEXT")
                        .HasColumnName("player_id");

                    b.Property<int>("ResourceType")
                        .HasColumnType("INTEGER")
                        .HasColumnName("resource_type");

                    b.Property<int>("Amount")
                        .HasColumnType("INTEGER")
                        .HasColumnName("amount");

                    b.HasKey("PlayerId", "ResourceType")
                        .HasName("pk_resources");

                    b.ToTable("resources", (string)null);
                });

            modelBuilder.Entity("Game.Data.Resource", b =>
                {
                    b.HasOne("Game.Data.Player", "Player")
                        .WithMany("Resources")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_resources_players_player_id");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("Game.Data.Player", b =>
                {
                    b.Navigation("Resources");
                });
#pragma warning restore 612, 618
        }
    }
}