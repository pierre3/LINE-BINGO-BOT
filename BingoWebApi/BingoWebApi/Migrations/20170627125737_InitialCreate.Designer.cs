using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using BingoWebApi.Models;

namespace BingoWebApi.Migrations
{
    [DbContext(typeof(BingoApiContext))]
    [Migration("20170627125737_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BingoWebApi.Models.Card", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("GameId");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Cards");
                });

            modelBuilder.Entity("BingoWebApi.Models.CardCell", b =>
                {
                    b.Property<int>("CardId");

                    b.Property<int>("Index");

                    b.Property<bool>("IsOpen");

                    b.Property<int>("Number");

                    b.HasKey("CardId", "Index");

                    b.ToTable("CardCells");
                });

            modelBuilder.Entity("BingoWebApi.Models.DrawSource", b =>
                {
                    b.Property<int>("GameId");

                    b.Property<int>("Index");

                    b.Property<int>("Number");

                    b.HasKey("GameId", "Index");

                    b.ToTable("DrawSource");
                });

            modelBuilder.Entity("BingoWebApi.Models.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("AccessKey");

                    b.Property<DateTime>("CreatedTime")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DrawCount");

                    b.Property<string>("Keyword");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("BingoWebApi.Models.Card", b =>
                {
                    b.HasOne("BingoWebApi.Models.Game")
                        .WithMany("Cards")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BingoWebApi.Models.CardCell", b =>
                {
                    b.HasOne("BingoWebApi.Models.Card")
                        .WithMany("CardCells")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BingoWebApi.Models.DrawSource", b =>
                {
                    b.HasOne("BingoWebApi.Models.Game")
                        .WithMany("DrawSource")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
