using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lotofacil.Infraestrutura.Migracoes
{
    /// <inheritdoc />
    public partial class CriacaoConcursos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Concurso",
                columns: table => new
                {
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Dezenas = table.Column<string>(type: "TEXT", nullable: false),
                    OrdemSorteio = table.Column<string>(type: "TEXT", nullable: true),
                    Arrecadacao = table.Column<decimal>(type: "TEXT", nullable: false),
                    Acumulado15 = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstimativaProximo = table.Column<decimal>(type: "TEXT", nullable: false),
                    AcumuladoIndependencia = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concurso", x => x.Numero);
                });

            migrationBuilder.CreateTable(
                name: "ConcursoRateio",
                columns: table => new
                {
                    Faixa = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroConcurso = table.Column<int>(type: "INTEGER", nullable: false),
                    Ganhadores = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorPremio = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcursoRateio", x => new { x.NumeroConcurso, x.Faixa });
                    table.ForeignKey(
                        name: "FK_ConcursoRateio_Concurso_NumeroConcurso",
                        column: x => x.NumeroConcurso,
                        principalTable: "Concurso",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConcursoRateio");

            migrationBuilder.DropTable(
                name: "Concurso");
        }
    }
}
