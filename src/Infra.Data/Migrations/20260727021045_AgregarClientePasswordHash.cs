using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarClientePasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_TenantId",
                table: "Clientes");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Clientes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_TenantId_Email",
                table: "Clientes",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "\"PasswordHash\" IS NOT NULL AND \"Email\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_TenantId_Email",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Clientes");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_TenantId",
                table: "Clientes",
                column: "TenantId");
        }
    }
}
