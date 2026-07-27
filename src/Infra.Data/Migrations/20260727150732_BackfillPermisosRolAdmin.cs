using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillPermisosRolAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rol.Permisos es un snapshot (PermisosMask) tomado al crear el rol, no algo que
            // se recalcule cuando el enum Permiso gana un valor nuevo (ver AuthAppService.
            // SeedearRolesAsync / Permiso.Todos). El rol Admin (EsSistema=true) es además
            // no editable desde la UI (RolAppService.UpdateAsync bloquea EsSistema), así que
            // los tenants creados antes de que existiera GestionarListaEspera (1L << 12,
            // agregada en 20260726235853_AgregarListaEspera) quedaron con Admin sin ese
            // permiso, sin ninguna forma de arreglarlo desde el frontend. Este backfill
            // reafirma la invariante "Admin = Permiso.Todos" para todos los tenants existentes.
            // Permiso.Todos actual = 8191 (bits 0..12, ver Core.Domain/Enums/Permiso.cs).
            migrationBuilder.Sql(
                """
                UPDATE "Roles" SET "PermisosMask" = 8191 WHERE "EsSistema" = TRUE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
