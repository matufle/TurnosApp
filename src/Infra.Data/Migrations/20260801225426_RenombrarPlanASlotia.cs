using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarPlanASlotia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rebranding Turnify -> Slotia: renombra el plan ya sembrado por
            // AgregarSuscripcionMercadoPago en ambientes que ya corrieron esa migración
            // (esa migración quedó editada para sembrar 'Plan Slotia' directo en instalaciones
            // nuevas, pero eso no reescribe la fila que ya existe acá).
            migrationBuilder.Sql(
                """
                UPDATE "Planes" SET "Nombre" = 'Plan Slotia' WHERE "Nombre" = 'Plan Turnify';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Planes" SET "Nombre" = 'Plan Turnify' WHERE "Nombre" = 'Plan Slotia';
                """);
        }
    }
}
