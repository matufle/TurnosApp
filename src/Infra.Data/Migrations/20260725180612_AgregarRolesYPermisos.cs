using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRolesYPermisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Usuarios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            // Nullable por ahora: hay que seedear los Roles y backfillear los Usuarios existentes
            // antes de poder exigir NOT NULL (ver más abajo, después de crear la tabla Roles).
            migrationBuilder.AddColumn<int>(
                name: "RolId",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Recursos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PermisosMask = table.Column<long>(type: "bigint", nullable: false),
                    EsSistema = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_UsuarioId",
                table: "Recursos",
                column: "UsuarioId",
                unique: true,
                filter: "\"UsuarioId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId",
                table: "Roles",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recursos_Usuarios_UsuarioId",
                table: "Recursos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // ── Data migration: seedea los 3 roles default por cada Tenant existente y asigna
            // Admin a todos los Usuarios que ya existían (hoy hay como máximo 1 por tenant).
            // Los valores de PermisosMask son el bitmask literal de Permiso (Core.Domain.Enums):
            // Admin = Todos = 4095; Empleado = GestionarTurnos(2) | CrearCobros(64) = 66;
            // Recepcionista = VerAgendaCompleta(1) | GestionarTurnos(2) | GestionarClientes(4) | CrearCobros(64) = 71.
            migrationBuilder.Sql(
                """
                INSERT INTO "Roles" ("Nombre", "PermisosMask", "EsSistema", "TenantId")
                SELECT 'Admin', 4095, TRUE, "Id" FROM "Tenants";

                INSERT INTO "Roles" ("Nombre", "PermisosMask", "EsSistema", "TenantId")
                SELECT 'Empleado', 66, FALSE, "Id" FROM "Tenants";

                INSERT INTO "Roles" ("Nombre", "PermisosMask", "EsSistema", "TenantId")
                SELECT 'Recepcionista', 71, FALSE, "Id" FROM "Tenants";

                UPDATE "Usuarios" u
                SET "RolId" = r."Id",
                    "Nombre" = CASE WHEN u."Nombre" IS NULL OR u."Nombre" = ''
                                     THEN split_part(u."Email", '@', 1)
                                     ELSE u."Nombre" END
                FROM "Roles" r
                WHERE r."TenantId" = u."TenantId" AND r."Nombre" = 'Admin';
                """);

            migrationBuilder.AlterColumn<int>(
                name: "RolId",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Roles_RolId",
                table: "Usuarios",
                column: "RolId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recursos_Usuarios_UsuarioId",
                table: "Recursos");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Roles_RolId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Recursos_UsuarioId",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RolId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Recursos");
        }
    }
}
