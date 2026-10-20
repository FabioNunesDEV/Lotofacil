using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lotofacil.Infraestrutura.Migracoes
{
    /// <inheritdoc />
    public partial class CriacaoJogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jogo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumeroConcurso = table.Column<int>(type: "INTEGER", nullable: false),
                    Dezenas = table.Column<string>(type: "TEXT", nullable: false),
                    CadastradoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jogo_NumeroConcurso_Dezenas",
                table: "Jogo",
                columns: new[] { "NumeroConcurso", "Dezenas" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jogo");
        }
    }
}
