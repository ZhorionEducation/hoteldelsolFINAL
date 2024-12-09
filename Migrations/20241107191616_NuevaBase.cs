using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel.Migrations
{
    /// <inheritdoc />
    public partial class NuevaBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Departam__3214EC070B695FD0", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genero",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Genero__3214EC07B9EAF31C", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Habitaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    NumeroHabitacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    PrecioPorNoche = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Habitaci__3214EC07A706F2DA", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Permisos__3214EC07F2DF7F7C", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__3214EC0772389402", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiciosAdicionales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Servicio__3214EC07323488B9", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposDocumento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TipoDocumento__3214EC073188D733", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Municipios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartamentoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Municipi__3214EC071F4EF517", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Municipio__Depar__693CA210",
                        column: x => x.DepartamentoId,
                        principalTable: "Departamentos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CorreoElectronico = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Contrasena = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    GeneroId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImagenPerfil = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FechaRegistro = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "(getdate())"),
                    Activo = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    NumeroDocumento = table.Column<long>(type: "bigint", nullable: true),
                    TipoDocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuarios__3214EC073188D733", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Usuarios__Genero__5629CD9C",
                        column: x => x.GeneroId,
                        principalTable: "Genero",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Usuarios__RolId__571DF1D5",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Usuarios__TipoDocumento__5812160E",
                        column: x => x.TipoDocumentoId,
                        principalTable: "TiposDocumento",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HabitacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    PrecioTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumeroAcompanantes = table.Column<int>(type: "int", nullable: true),
                    FechaReserva = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Reservas__3214EC075ADF7209", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Reservas__Habita__6EF57B66",
                        column: x => x.HabitacionId,
                        principalTable: "Habitaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Reservas__Usuari__6E01572D",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comodidades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Imagen = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReservaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Comodida__3214EC07444DCBF7", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comodidades_Reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    ReservaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MetodoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaPago = table.Column<DateOnly>(type: "date", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Pagos__3214EC070D022E1C", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Pagos__ReservaId__778AC167",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReservaServicios",
                columns: table => new
                {
                    ReservaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ReservaS__FEC3D9AFBCDC9544", x => new { x.ReservaId, x.ServicioId });
                    table.ForeignKey(
                        name: "FK__ReservaSe__Reser__7A672E12",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ReservaSe__Servi__7B5B524B",
                        column: x => x.ServicioId,
                        principalTable: "ServiciosAdicionales",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HabitacionComodidades",
                columns: table => new
                {
                    HabitacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComodidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HabitacionComodidade", x => new { x.HabitacionId, x.ComodidadId });
                    table.ForeignKey(
                        name: "FK__HabitacionComodidade__ComodidadId",
                        column: x => x.ComodidadId,
                        principalTable: "Comodidades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__HabitacionComodidade__HabitacionId",
                        column: x => x.HabitacionId,
                        principalTable: "Habitaciones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comodidades_ReservaId",
                table: "Comodidades",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "UQ__Comodida__75E3EFCF13DB151D",
                table: "Comodidades",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Genero__75E3EFCF4D1426A7",
                table: "Genero",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HabitacionComodidades_ComodidadId",
                table: "HabitacionComodidades",
                column: "ComodidadId");

            migrationBuilder.CreateIndex(
                name: "UQ__Habitaci__08B8232FF46DA6B5",
                table: "Habitaciones",
                column: "NumeroHabitacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Municipios_DepartamentoId",
                table: "Municipios",
                column: "DepartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ReservaId",
                table: "Pagos",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "UQ__Permisos__75E3EFCF02A16545",
                table: "Permisos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_HabitacionId",
                table: "Reservas",
                column: "HabitacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_UsuarioId",
                table: "Reservas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservaServicios_ServicioId",
                table: "ReservaServicios",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "UQ__Roles__75E3EFCFD93DC719",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Servicio__75E3EFCF73369AB9",
                table: "ServiciosAdicionales",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__TipoDocumento__75E3EFCF13DB151D",
                table: "TiposDocumento",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_GeneroId",
                table: "Usuarios",
                column: "GeneroId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TipoDocumentoId",
                table: "Usuarios",
                column: "TipoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "UQ__Usuarios__531402F3E6A41C06",
                table: "Usuarios",
                column: "CorreoElectronico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Usuarios__6B0F5AE03D0126D5",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HabitacionComodidades");

            migrationBuilder.DropTable(
                name: "Municipios");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "ReservaServicios");

            migrationBuilder.DropTable(
                name: "Comodidades");

            migrationBuilder.DropTable(
                name: "Departamentos");

            migrationBuilder.DropTable(
                name: "ServiciosAdicionales");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Habitaciones");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Genero");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "TiposDocumento");
        }
    }
}
