using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVerificacionEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmado",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenConfirmacionEmail",
                table: "Usuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenConfirmacionExpira",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ClienteId",
                table: "Notificaciones",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmado",
                table: "Clientes",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenConfirmacionEmail",
                table: "Clientes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenConfirmacionExpira",
                table: "Clientes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenConfirmacionEmail",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenConfirmacionExpira",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EmailConfirmado",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TokenConfirmacionEmail",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TokenConfirmacionExpira",
                table: "Clientes");

            migrationBuilder.AlterColumn<int>(
                name: "ClienteId",
                table: "Notificaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
