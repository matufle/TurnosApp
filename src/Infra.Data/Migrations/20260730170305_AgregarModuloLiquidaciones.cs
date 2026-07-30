using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModuloLiquidaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FrecuenciaLiquidacion",
                table: "Tenants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Mensual");

            migrationBuilder.CreateTable(
                name: "Liquidaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecursoId = table.Column<int>(type: "integer", nullable: false),
                    PeriodoDesde = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodoHasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioPagoId = table.Column<int>(type: "integer", nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Liquidaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Liquidaciones_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Liquidaciones_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Liquidaciones_Usuarios_UsuarioPagoId",
                        column: x => x.UsuarioPagoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReglasComision",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecursoId = table.Column<int>(type: "integer", nullable: false),
                    ServicioId = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasComision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReglasComision_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglasComision_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglasComision_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdelantosProfesional",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecursoId = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LiquidacionId = table.Column<int>(type: "integer", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdelantosProfesional", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdelantosProfesional_Liquidaciones_LiquidacionId",
                        column: x => x.LiquidacionId,
                        principalTable: "Liquidaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AdelantosProfesional_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdelantosProfesional_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LiquidacionDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiquidacionId = table.Column<int>(type: "integer", nullable: false),
                    TurnoId = table.Column<int>(type: "integer", nullable: false),
                    ServicioId = table.Column<int>(type: "integer", nullable: false),
                    PrecioBaseAplicado = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TipoComisionSnapshot = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ValorComisionSnapshot = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidacionDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidacionDetalles_Liquidaciones_LiquidacionId",
                        column: x => x.LiquidacionId,
                        principalTable: "Liquidaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiquidacionDetalles_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LiquidacionDetalles_Turnos_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdelantosProfesional_LiquidacionId",
                table: "AdelantosProfesional",
                column: "LiquidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdelantosProfesional_RecursoId",
                table: "AdelantosProfesional",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_AdelantosProfesional_TenantId",
                table: "AdelantosProfesional",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionDetalles_LiquidacionId",
                table: "LiquidacionDetalles",
                column: "LiquidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionDetalles_ServicioId",
                table: "LiquidacionDetalles",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionDetalles_TurnoId",
                table: "LiquidacionDetalles",
                column: "TurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidaciones_RecursoId",
                table: "Liquidaciones",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidaciones_TenantId_RecursoId_PeriodoDesde_PeriodoHasta",
                table: "Liquidaciones",
                columns: new[] { "TenantId", "RecursoId", "PeriodoDesde", "PeriodoHasta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Liquidaciones_UsuarioPagoId",
                table: "Liquidaciones",
                column: "UsuarioPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasComision_RecursoId",
                table: "ReglasComision",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasComision_ServicioId",
                table: "ReglasComision",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasComision_TenantId_RecursoId",
                table: "ReglasComision",
                columns: new[] { "TenantId", "RecursoId" },
                unique: true,
                filter: "\"Activo\" = true AND \"ServicioId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasComision_TenantId_RecursoId_ServicioId",
                table: "ReglasComision",
                columns: new[] { "TenantId", "RecursoId", "ServicioId" },
                unique: true,
                filter: "\"Activo\" = true AND \"ServicioId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdelantosProfesional");

            migrationBuilder.DropTable(
                name: "LiquidacionDetalles");

            migrationBuilder.DropTable(
                name: "ReglasComision");

            migrationBuilder.DropTable(
                name: "Liquidaciones");

            migrationBuilder.DropColumn(
                name: "FrecuenciaLiquidacion",
                table: "Tenants");
        }
    }
}
