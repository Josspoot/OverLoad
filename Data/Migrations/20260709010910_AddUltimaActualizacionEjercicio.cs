using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OverLoad.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUltimaActualizacionEjercicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaActualizacion",
                table: "Ejercicios",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UltimaActualizacion",
                table: "Ejercicios");
        }
    }
}
