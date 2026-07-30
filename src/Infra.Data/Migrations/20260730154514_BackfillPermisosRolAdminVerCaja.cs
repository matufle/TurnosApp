using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillPermisosRolAdminVerCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mismo razonamiento que 20260730150115_BackfillPermisosRolAdminCaja: VerCaja
            // (1L << 15) se agregó después de que tenants existentes ya tuvieran su Admin sembrado.
            // Permiso.Todos actual = 65535 (bits 0..15, ver Core.Domain/Enums/Permiso.cs).
            migrationBuilder.Sql(
                """
                UPDATE "Roles" SET "PermisosMask" = 65535 WHERE "EsSistema" = TRUE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
