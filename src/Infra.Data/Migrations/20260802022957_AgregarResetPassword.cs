using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnosApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarResetPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TokenResetPassword",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenResetPasswordExpira",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenResetPassword",
                table: "Clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenResetPasswordExpira",
                table: "Clientes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenResetPassword",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenResetPasswordExpira",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenResetPassword",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TokenResetPasswordExpira",
                table: "Clientes");
        }
    }
}
