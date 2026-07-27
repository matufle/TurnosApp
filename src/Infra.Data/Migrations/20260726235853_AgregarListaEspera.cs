using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarListaEspera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListasEspera",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    RecursoId = table.Column<int>(type: "integer", nullable: false),
                    ServicioId = table.Column<int>(type: "integer", nullable: true),
                    FechaDesde = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaHasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NotificadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListasEspera", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListasEspera_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListasEspera_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListasEspera_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListasEspera_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListasEspera_ClienteId",
                table: "ListasEspera",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ListasEspera_RecursoId_Estado",
                table: "ListasEspera",
                columns: new[] { "RecursoId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_ListasEspera_ServicioId",
                table: "ListasEspera",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ListasEspera_TenantId",
                table: "ListasEspera",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListasEspera");
        }
    }
}
