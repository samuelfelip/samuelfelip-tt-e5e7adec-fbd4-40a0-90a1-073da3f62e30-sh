using HighPerformance.Ingest.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HighPerformance.Ingest.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.5")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("HighPerformance.Ingest.Domain.Entities.ScoreEntry", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                b.Property<string>("UserId")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("character varying(200)");

                b.Property<long>("Score")
                    .HasColumnType("bigint");

                b.Property<DateTime>("Timestamp")
                    .HasColumnType("timestamp with time zone");

                b.Property<uint>("Version")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("xid")
                    .HasColumnName("xmin");

                b.HasKey("Id");

                b.HasIndex("UserId", "Timestamp")
                    .HasDatabaseName("IX_ScoreEntries_UserId_Timestamp");

                b.ToTable("ScoreEntries");
            });
#pragma warning restore 612, 618
    }
}
