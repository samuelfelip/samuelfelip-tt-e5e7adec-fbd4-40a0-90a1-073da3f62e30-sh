using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HighPerformance.Ingest.Infrastructure.Persistence.Migrations;

public partial class AddScoreEntries : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ScoreEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Score = table.Column<long>(type: "bigint", nullable: false),
                Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ScoreEntries", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ScoreEntries_UserId_Timestamp",
            table: "ScoreEntries",
            columns: new[] { "UserId", "Timestamp" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ScoreEntries");
    }
}
