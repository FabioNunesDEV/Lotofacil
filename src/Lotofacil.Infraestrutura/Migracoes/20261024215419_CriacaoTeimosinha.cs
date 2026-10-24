using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lotofacil.Infraestrutura.Migracoes
{
    /// <inheritdoc />
    public partial class CriacaoTeimosinha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeimosinhaId",
                table: "Jogo",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Teimosinha",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false),
                    UltimoConcursoCriado = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teimosinha", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeimosinhaJogo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Dezenas = table.Column<string>(type: "TEXT", nullable: false),
                    ConcursoInicial = table.Column<int>(type: "INTEGER", nullable: false),
                    TeimosinhaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeimosinhaJogo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeimosinhaJogo_Teimosinha_TeimosinhaId",
                        column: x => x.TeimosinhaId,
                        principalTable: "Teimosinha",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jogo_TeimosinhaId",
                table: "Jogo",
                column: "TeimosinhaId");

            migrationBuilder.CreateIndex(
                name: "IX_TeimosinhaJogo_TeimosinhaId",
                table: "TeimosinhaJogo",
                column: "TeimosinhaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jogo_Teimosinha_TeimosinhaId",
                table: "Jogo",
                column: "TeimosinhaId",
                principalTable: "Teimosinha",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jogo_Teimosinha_TeimosinhaId",
                table: "Jogo");

            migrationBuilder.DropTable(
                name: "TeimosinhaJogo");

            migrationBuilder.DropTable(
                name: "Teimosinha");

            migrationBuilder.DropIndex(
                name: "IX_Jogo_TeimosinhaId",
                table: "Jogo");

            migrationBuilder.DropColumn(
                name: "TeimosinhaId",
                table: "Jogo");
        }
    }
}
