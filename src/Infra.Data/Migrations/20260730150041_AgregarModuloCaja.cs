using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModuloCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsEfectivo",
                table: "MetodosPago",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Backfill best-effort: no había forma de saber esto antes de este cambio, así que
            // adivinamos por nombre para no dejar en 0 los métodos "Efectivo" ya cargados por
            // los tenants existentes. El tenant puede corregirlo a mano desde el formulario.
            migrationBuilder.Sql(
                """
                UPDATE "MetodosPago" SET "EsEfectivo" = TRUE WHERE lower("Nombre") = 'efectivo';
                """);

            migrationBuilder.CreateTable(
                name: "SesionesCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioAperturaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioCierreId = table.Column<int>(type: "integer", nullable: true),
                    FechaApertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MontoInicial = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MontoFinalDeclarado = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CierreForzado = table.Column<bool>(type: "boolean", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionesCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SesionesCaja_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SesionesCaja_Usuarios_UsuarioAperturaId",
                        column: x => x.UsuarioAperturaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SesionesCaja_Usuarios_UsuarioCierreId",
                        column: x => x.UsuarioCierreId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SesionCajaId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MetodoPagoId = table.Column<int>(type: "integer", nullable: true),
                    NombreMetodoPagoSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EsEfectivoSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    CobroId = table.Column<int>(type: "integer", nullable: true),
                    MovimientoOrigenId = table.Column<int>(type: "integer", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_Cobros_CobroId",
                        column: x => x.CobroId,
                        principalTable: "Cobros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_MetodosPago_MetodoPagoId",
                        column: x => x.MetodoPagoId,
                        principalTable: "MetodosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_MovimientosCaja_MovimientoOrigenId",
                        column: x => x.MovimientoOrigenId,
                        principalTable: "MovimientosCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_SesionesCaja_SesionCajaId",
                        column: x => x.SesionCajaId,
                        principalTable: "SesionesCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_CobroId",
                table: "MovimientosCaja",
                column: "CobroId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_MetodoPagoId",
                table: "MovimientosCaja",
                column: "MetodoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_MovimientoOrigenId",
                table: "MovimientosCaja",
                column: "MovimientoOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_SesionCajaId",
                table: "MovimientosCaja",
                column: "SesionCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_TenantId",
                table: "MovimientosCaja",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_UsuarioId",
                table: "MovimientosCaja",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesCaja_TenantId",
                table: "SesionesCaja",
                column: "TenantId",
                unique: true,
                filter: "\"Estado\" = 'Abierta'");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesCaja_UsuarioAperturaId",
                table: "SesionesCaja",
                column: "UsuarioAperturaId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesCaja_UsuarioCierreId",
                table: "SesionesCaja",
                column: "UsuarioCierreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosCaja");

            migrationBuilder.DropTable(
                name: "SesionesCaja");

            migrationBuilder.DropColumn(
                name: "EsEfectivo",
                table: "MetodosPago");
        }
    }
}
