using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSuscripcionMercadoPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsGrandfathered",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EstadoSuscripcion",
                table: "Tenants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Trial");

            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoPreapprovalId",
                table: "Tenants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlanId",
                table: "Tenants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuscripcionVenceEn",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Planes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrecioMensual = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    TrialDias = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebhookEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotificacionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProcesadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_PlanId",
                table: "Tenants",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEvents_NotificacionId",
                table: "WebhookEvents",
                column: "NotificacionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Planes_PlanId",
                table: "Tenants",
                column: "PlanId",
                principalTable: "Planes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Seedea el único plan flat (ver decisión de producto: ARS $50.000/mes, trial de
            // 30 días, sin tiers). A diferencia de lo que hubiera sido con Stripe, no hace
            // falta cachear un id externo acá — la API de Preapproval "sin plan asociado" de
            // Mercado Pago recibe el monto inline en cada alta (ver IMercadoPagoService).
            migrationBuilder.Sql(
                """
                INSERT INTO "Planes" ("Nombre", "PrecioMensual", "TrialDias", "Activo")
                VALUES ('Plan Slotia', 50000, 30, TRUE);
                """);

            // Grandfathering total: todo tenant que ya existía antes de este deploy (todos
            // los que hay en la tabla en este momento, ya que esta migración corre una sola
            // vez) queda marcado como activo sin pasar nunca por Mercado Pago — decisión de
            // producto confirmada con Mateo (ver memoria del proyecto). Solo los tenants
            // registrados DESPUÉS de este deploy (AuthAppService.RegisterAsync) entran al
            // flujo real de trial → suscripción → gating.
            migrationBuilder.Sql(
                """
                UPDATE "Tenants"
                SET "EsGrandfathered" = TRUE,
                    "EstadoSuscripcion" = 'Activa',
                    "PlanId" = (SELECT "Id" FROM "Planes" WHERE "Nombre" = 'Plan Slotia' LIMIT 1);
                """);

            // Mismo gotcha documentado en CLAUDE.md (ver 20260727150732_BackfillPermisosRolAdmin):
            // Rol.PermisosMask de un rol EsSistema es un snapshot congelado al crear el tenant,
            // no se recalcula cuando Permiso gana un valor nuevo. Sin este backfill, todo tenant
            // creado antes de este deploy nunca podría otorgar GestionarSuscripcion a nadie
            // (ni siquiera a Admin) porque Admin no es editable desde la UI.
            // Permiso.Todos actual = 524287 (bits 0..18, ver Core.Domain/Enums/Permiso.cs).
            migrationBuilder.Sql(
                """
                UPDATE "Roles" SET "PermisosMask" = 524287 WHERE "EsSistema" = TRUE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Planes_PlanId",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "Planes");

            migrationBuilder.DropTable(
                name: "WebhookEvents");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_PlanId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "EsGrandfathered",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "EstadoSuscripcion",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "MercadoPagoPreapprovalId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SuscripcionVenceEn",
                table: "Tenants");
        }
    }
}
