using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedearMetodoPagoEfectivoTenantsExistentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AuthAppService.RegisterAsync ahora siembra "Efectivo" para tenants nuevos, pero
            // los tenants ya existentes se quedaron sin ningún MetodoPago marcado EsEfectivo —
            // Caja quedaba inutilizable para efectivo hasta que alguien lo creara a mano.
            migrationBuilder.Sql(
                """
                INSERT INTO "MetodosPago" ("TenantId", "Nombre", "TipoModificador", "PorcentajeModificador", "PorcentajeComision", "EsEfectivo", "Activo")
                SELECT t."Id", 'Efectivo', 'Ninguno', 0, 0, TRUE, TRUE
                FROM "Tenants" t
                WHERE NOT EXISTS (
                    SELECT 1 FROM "MetodosPago" mp WHERE mp."TenantId" = t."Id" AND mp."EsEfectivo" = TRUE
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
