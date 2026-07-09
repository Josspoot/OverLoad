using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OverLoad.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistroAlimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrosAlimentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Marca = table.Column<string>(type: "TEXT", nullable: false),
                    Gramos = table.Column<double>(type: "REAL", nullable: false),
                    Calorias = table.Column<double>(type: "REAL", nullable: false),
                    Proteina = table.Column<double>(type: "REAL", nullable: false),
                    Carbohidrato = table.Column<double>(type: "REAL", nullable: false),
                    Grasa = table.Column<double>(type: "REAL", nullable: false),
                    CodigoBarras = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAlimentos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosAlimentos");
        }
    }
}
