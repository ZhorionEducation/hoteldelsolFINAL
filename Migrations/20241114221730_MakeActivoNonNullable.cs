using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel.Migrations
{
    /// <inheritdoc />
    public partial class MakeActivoNonNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK__Habitaci__3214EC07A706F2DA",
                table: "Habitaciones");

            migrationBuilder.DropIndex(
                name: "UQ__Habitaci__08B8232FF46DA6B5",
                table: "Habitaciones");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroHabitacion",
                table: "Habitaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Habitaciones",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Habitaciones",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Habitaciones",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "(newid())");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Habitaciones",
                table: "Habitaciones",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Habitaciones",
                table: "Habitaciones");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroHabitacion",
                table: "Habitaciones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Habitaciones",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Habitaciones",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Habitaciones",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "(newid())",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Habitaci__3214EC07A706F2DA",
                table: "Habitaciones",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "UQ__Habitaci__08B8232FF46DA6B5",
                table: "Habitaciones",
                column: "NumeroHabitacion",
                unique: true);
        }
    }
}
