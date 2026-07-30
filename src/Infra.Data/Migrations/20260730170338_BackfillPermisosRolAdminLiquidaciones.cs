using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillPermisosRolAdminLiquidaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mismo razonamiento que 20260730154514_BackfillPermisosRolAdminVerCaja:
            // VerLiquidaciones (1L << 16) y GestionarLiquidaciones (1L << 17) se agregaron
            // después de que tenants existentes ya tuvieran su Admin sembrado.
            // Permiso.Todos actual = 262143 (bits 0..17, ver Core.Domain/Enums/Permiso.cs).
            migrationBuilder.Sql(
                """
                UPDATE "Roles" SET "PermisosMask" = 262143 WHERE "EsSistema" = TRUE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
