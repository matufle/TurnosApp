using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillPermisosRolAdminCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mismo razonamiento que 20260727150732_BackfillPermisosRolAdmin: Rol.PermisosMask
            // es un snapshot tomado al crear el rol, no algo que se recalcule cuando Permiso
            // gana un valor nuevo. GestionarCaja/ForzarCierreCaja (1L << 13, 1L << 14) se
            // agregaron después de que tenants existentes ya tuvieran su Admin sembrado.
            // Permiso.Todos actual = 32767 (bits 0..14, ver Core.Domain/Enums/Permiso.cs).
            migrationBuilder.Sql(
                """
                UPDATE "Roles" SET "PermisosMask" = 32767 WHERE "EsSistema" = TRUE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
