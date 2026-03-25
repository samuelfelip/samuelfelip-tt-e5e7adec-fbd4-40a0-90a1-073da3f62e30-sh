using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HighPerformance.Ingest.Infrastructure.Persistence.Migrations;

[Migration("202603260001_RemoveProductos")]
public partial class RemoveProductos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Productos");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Productos",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Stock = table.Column<int>(type: "integer", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Productos", x => x.Id);
            });
    }
}
