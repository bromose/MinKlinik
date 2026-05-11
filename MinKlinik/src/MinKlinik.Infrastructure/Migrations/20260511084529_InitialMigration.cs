using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinKlinik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Behandlere",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Navn = table.Column<string>(type: "TEXT", nullable: false),
                    Speciale = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Behandlere", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Behandlingstyper",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Navn = table.Column<string>(type: "TEXT", nullable: false),
                    EgenbetalingsBeløb = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Behandlingstyper", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Konsultationer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BehandlingstypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PatientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BehandlerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Notat = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    EgenbetalingsBeløb = table.Column<string>(type: "TEXT", nullable: false),
                    Tidspunkt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Konsultationer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patienter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Navn = table.Column<string>(type: "TEXT", nullable: false),
                    CprNummer = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patienter", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Behandlere");

            migrationBuilder.DropTable(
                name: "Behandlingstyper");

            migrationBuilder.DropTable(
                name: "Konsultationer");

            migrationBuilder.DropTable(
                name: "Patienter");
        }
    }
}
