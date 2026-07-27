using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNotificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EstadoEnvio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TurnoId = table.Column<int>(type: "integer", nullable: true),
                    ListaEsperaId = table.Column<int>(type: "integer", nullable: true),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    DestinatarioEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Asunto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CuerpoHtml = table.Column<string>(type: "text", nullable: false),
                    ProgramadaPara = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnviadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IntentosEnvio = table.Column<int>(type: "integer", nullable: false),
                    UltimoError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReclamadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReclamadaPorRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notificaciones_ListasEspera_ListaEsperaId",
                        column: x => x.ListaEsperaId,
                        principalTable: "ListasEspera",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Turnos_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_ClienteId",
                table: "Notificaciones",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_EstadoEnvio_ProgramadaPara",
                table: "Notificaciones",
                columns: new[] { "EstadoEnvio", "ProgramadaPara" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_ListaEsperaId",
                table: "Notificaciones",
                column: "ListaEsperaId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_ReclamadaPorRunId",
                table: "Notificaciones",
                column: "ReclamadaPorRunId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_TenantId",
                table: "Notificaciones",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_TurnoId_EstadoEnvio",
                table: "Notificaciones",
                columns: new[] { "TurnoId", "EstadoEnvio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificaciones");
        }
    }
}
