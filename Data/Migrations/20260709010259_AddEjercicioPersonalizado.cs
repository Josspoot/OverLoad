using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OverLoad.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEjercicioPersonalizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EjerciciosPersonalizados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Grupo = table.Column<string>(type: "TEXT", nullable: false),
                    ComoHacerlo = table.Column<string>(type: "TEXT", nullable: false),
                    QueSeDebeSentir = table.Column<string>(type: "TEXT", nullable: false),
                    Recomendaciones = table.Column<string>(type: "TEXT", nullable: false),
                    EquipoExtra = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EjerciciosPersonalizados", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EjerciciosPersonalizados");
        }
    }
}
