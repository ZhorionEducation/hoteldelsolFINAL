using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Models
{
    public partial class HotelContext : DbContext
    {
        public HotelContext()
        {
        }

        public HotelContext(DbContextOptions<HotelContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Comodidade> Comodidades { get; set; }
        public virtual DbSet<Departamento> Departamentos { get; set; }
        public virtual DbSet<Genero> Generos { get; set; }
        public virtual DbSet<Habitacione> Habitaciones { get; set; }
        public virtual DbSet<Municipio> Municipios { get; set; }
        public virtual DbSet<Pago> Pagos { get; set; }
        public virtual DbSet<Permiso> Permisos { get; set; }
        public virtual DbSet<Reserva> Reservas { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<ServiciosAdicionale> ServiciosAdicionales { get; set; }
        public virtual DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<TipoDocumento> TiposDocumento { get; set; }
        public virtual DbSet<Huesped> Huespedes { get; set; }
        public virtual DbSet<Calificacion> Calificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comodidade>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Comodida__3214EC07444DCBF7");

                entity.HasIndex(e => e.Nombre, "UQ__Comodida__75E3EFCF13DB151D").IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.Descripcion).HasMaxLength(255);
                entity.Property(e => e.Imagen).HasMaxLength(255);
                entity.Property(e => e.Nombre).HasMaxLength(100);
                entity.Property(e => e.Precio).HasColumnType("decimal(18, 2)");

                entity.HasMany(d => d.Habitaciones).WithMany(p => p.Comodidades)
                    .UsingEntity<Dictionary<string, object>>(
                        "HabitacionComodidade",
                        r => r.HasOne<Habitacione>().WithMany()
                            .HasForeignKey("HabitacionId")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("FK__HabitacionComodidade__HabitacionId"),
                        l => l.HasOne<Comodidade>().WithMany()
                            .HasForeignKey("ComodidadId")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("FK__HabitacionComodidade__ComodidadId"),
                        j =>
                        {
                            j.HasKey("HabitacionId", "ComodidadId").HasName("PK__HabitacionComodidade");
                            j.ToTable("HabitacionComodidades");
                        });
            });

            modelBuilder.Entity<Departamento>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Departam__3214EC070B695FD0");

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Nombre).HasMaxLength(100);
            });

            modelBuilder.Entity<Genero>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Genero__3214EC07B9EAF31C");

                entity.ToTable("Genero");

                entity.HasIndex(e => e.Nombre, "UQ__Genero__75E3EFCF4D1426A7").IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.Nombre).HasMaxLength(50);
            });

            modelBuilder.Entity<Habitacione>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.NumeroHabitacion)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Descripcion).HasMaxLength(500);

                entity.Property(e => e.Capacidad).IsRequired();

                entity.Property(e => e.PrecioPorNoche).IsRequired();

                entity.Property(e => e.TipoHabitacion)
                .HasMaxLength(50);

                entity.Property(e => e.Imagen).HasMaxLength(255);

                entity.Property(e => e.Activo).IsRequired();

                entity.HasMany(d => d.Comodidades)
                    .WithMany(p => p.Habitaciones)
                    .UsingEntity<Dictionary<string, object>>(
                        "HabitacionComodidade",
                        r => r.HasOne<Comodidade>().WithMany().HasForeignKey("ComodidadId"),
                        l => l.HasOne<Habitacione>().WithMany().HasForeignKey("HabitacionId"),
                        je =>
                        {
                            je.HasKey("HabitacionId", "ComodidadId");
                        });
            });

            modelBuilder.Entity<Municipio>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Municipi__3214EC071F4EF517");

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Nombre).HasMaxLength(100);

                entity.HasOne(d => d.Departamento).WithMany(p => p.Municipios)
                    .HasForeignKey(d => d.DepartamentoId)
                    .HasConstraintName("FK__Municipio__Depar__693CA210");
            });

            modelBuilder.Entity<Pago>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Pagos__3214EC070D022E1C");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.Estado).HasMaxLength(50);
                entity.Property(e => e.MetodoPago).HasMaxLength(50);
                entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Reserva).WithMany(p => p.Pagos)
                    .HasForeignKey(d => d.ReservaId)
                    .HasConstraintName("FK__Pagos__ReservaId__778AC167");
            });

            modelBuilder.Entity<Permiso>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Permisos__3214EC07F2DF7F7C");

                entity.HasIndex(e => e.Nombre, "UQ__Permisos__75E3EFCF02A16545").IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.Nombre).HasMaxLength(50);
            });

            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Reservas__3214EC075ADF7209");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.FechaReserva).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.PrecioTotal).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Habitacion).WithMany(p => p.Reservas)
                    .HasForeignKey(d => d.HabitacionId)
                    .HasConstraintName("FK__Reservas__Habita__6EF57B66");

                entity.HasOne(d => d.Usuario).WithMany(p => p.Reservas)
                    .HasForeignKey(d => d.UsuarioId)
                    .HasConstraintName("FK__Reservas__Usuari__6E01572D");

                entity.HasMany(d => d.Servicios).WithMany(p => p.Reservas)
                    .UsingEntity<Dictionary<string, object>>(
                        "ReservaServicio",
                        r => r.HasOne<ServiciosAdicionale>().WithMany()
                            .HasForeignKey("ServicioId")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("FK__ReservaSe__Servi__7B5B524B"),
                        l => l.HasOne<Reserva>().WithMany()
                            .HasForeignKey("ReservaId")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("FK__ReservaSe__Reser__7A672E12"),
                        j =>
                        {
                            j.HasKey("ReservaId", "ServicioId").HasName("PK__ReservaS__FEC3D9AFBCDC9544");
                            j.ToTable("ReservaServicios");
                        });

                entity.HasMany(d => d.Huespedes).WithOne(p => p.Reserva)
                    .HasForeignKey(d => d.ReservaId)
                    .HasConstraintName("FK__Huespedes__ReservaId__7C4F7684");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC0772389402");

                entity.HasIndex(e => e.Nombre, "UQ__Roles__75E3EFCFD93DC719").IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.Nombre).HasMaxLength(50);
                entity.Property(e => e.Estado).IsRequired();

                entity.HasMany(d => d.Permisos)
                    .WithMany(p => p.Roles)
                    .UsingEntity<Dictionary<string, object>>(
                        "RolePermiso",
                        r => r.HasOne<Permiso>().WithMany().HasForeignKey("PermisoId"),
                        l => l.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                        j =>
                        {
                            j.HasKey("RoleId", "PermisoId");
                            j.ToTable("RolePermiso");
                        });
            });

            modelBuilder.Entity<ServiciosAdicionale>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Servicio__3214EC07323488B9");

                entity.HasIndex(e => e.Nombre, "UQ__Servicio__75E3EFCF73369AB9").IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.Descripcion).HasMaxLength(255);
                entity.Property(e => e.Imagen).HasMaxLength(255);
                entity.Property(e => e.Nombre).HasMaxLength(100);
                entity.Property(e => e.Precio).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<TipoDocumento>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__TipoDocumento__3214EC073188D733");

                entity.HasIndex(e => e.Nombre, "UQ__TipoDocumento__75E3EFCF13DB151D").IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.Nombre).HasMaxLength(100);
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC073188D733");

                entity.HasIndex(e => e.CorreoElectronico, "UQ__Usuarios__531402F3E6A41C06").IsUnique();

                entity.HasIndex(e => e.NombreUsuario, "UQ__Usuarios__6B0F5AE03D0126D5").IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.Apellido).HasMaxLength(100);
                entity.Property(e => e.Contrasena).HasMaxLength(255);
                entity.Property(e => e.CorreoElectronico).HasMaxLength(100);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.ImagenPerfil).HasMaxLength(255);
                entity.Property(e => e.Nombre).HasMaxLength(100);
                entity.Property(e => e.NombreUsuario).HasMaxLength(50);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.NumeroDocumento).HasColumnType("bigint");
                entity.Property(e => e.TipoDocumentoId).HasColumnType("uniqueidentifier");

                entity.HasOne(d => d.Genero).WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.GeneroId)
                    .HasConstraintName("FK__Usuarios__Genero__5629CD9C");

                entity.HasOne(d => d.Rol)
                    .WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.RolId)
                    .HasConstraintName("FK__Usuarios__RolId__571DF1D5");

                entity.HasOne(d => d.TipoDocumento).WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.TipoDocumentoId)
                    .HasConstraintName("FK__Usuarios__TipoDocumento__5812160E");
            });

            modelBuilder.Entity<Huesped>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Huesped__3214EC07A706F2DA");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.DocumentoIdentidad).HasMaxLength(50).IsRequired();

                entity.HasOne(d => d.Reserva).WithMany(p => p.Huespedes)
                    .HasForeignKey(d => d.ReservaId)
                    .HasConstraintName("FK__Huespedes__ReservaId__7C4F7684");
            });

            modelBuilder.Entity<Calificacion>(entity =>
    {
        entity.HasKey(e => e.Id).HasName("PK__Calificacion__3214EC07");

        entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
        entity.Property(e => e.CalificacionServicio).IsRequired();
        entity.Property(e => e.CalificacionHotel).IsRequired();
        entity.Property(e => e.CalificacionHabitacion).IsRequired();
        entity.Property(e => e.FechaCalificacion).HasDefaultValueSql("(getdate())");

        entity.HasOne(d => d.Reserva)
            .WithMany(p => p.Calificaciones)
            .HasForeignKey(d => d.ReservaId)
            .HasConstraintName("FK__Calificacion__ReservaId");

        entity.HasOne(d => d.Habitacion) // Nueva relación
        .WithMany(p => p.Calificaciones)
        .HasForeignKey(d => d.HabitacionId)
        .HasConstraintName("FK__Calificacion__HabitacionId");
    });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}