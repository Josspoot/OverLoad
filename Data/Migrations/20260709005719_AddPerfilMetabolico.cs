using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OverLoad.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerfilMetabolico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerfilesMetabolicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Actualizado = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Sexo = table.Column<int>(type: "INTEGER", nullable: false),
                    Edad = table.Column<int>(type: "INTEGER", nullable: false),
                    PesoKg = table.Column<double>(type: "REAL", nullable: false),
                    AlturaCm = table.Column<double>(type: "REAL", nullable: false),
                    NivelActividad = table.Column<int>(type: "INTEGER", nullable: false),
                    Objetivo = table.Column<int>(type: "INTEGER", nullable: false),
                    Formula = table.Column<int>(type: "INTEGER", nullable: false),
                    Tmb = table.Column<double>(type: "REAL", nullable: false),
                    Tdee = table.Column<double>(type: "REAL", nullable: false),
                    CaloriasObjetivo = table.Column<double>(type: "REAL", nullable: false),
                    ProteinaG = table.Column<int>(type: "INTEGER", nullable: false),
                    CarbohidratoG = table.Column<int>(type: "INTEGER", nullable: false),
                    GrasaG = table.Column<int>(type: "INTEGER", nullable: false),
                    NombreObjetivo = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilesMetabolicos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerfilesMetabolicos");
        }
    }
}
