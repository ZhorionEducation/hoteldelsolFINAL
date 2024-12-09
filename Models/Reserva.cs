using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Models;

public partial class Reserva
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El usuario es requerido")]

    public Guid? UsuarioId { get; set; }

    [Required(ErrorMessage = "La habitación es requerida")]
    public Guid? HabitacionId { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es requerida")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly FechaFin { get; set; }

    [Required(ErrorMessage = "El precio total es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio total debe ser un valor positivo")]
    public decimal PrecioTotal { get; set; }

    [Range(0, 5, ErrorMessage = "El número de acompañantes debe estar entre 0 y 5")]
    public int? NumeroAcompanantes { get; set; }

    public DateOnly? FechaReserva { get; set; }


    public virtual Habitacione? Habitacion { get; set; }

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual Usuario? Usuario { get; set; }

    public virtual ICollection<Comodidade> Comodidads { get; set; } = new List<Comodidade>();

    public virtual ICollection<ServiciosAdicionale> Servicios { get; set; } = new List<ServiciosAdicionale>();

    public virtual ICollection<Huesped> Huespedes { get; set; } = new List<Huesped>();

    public virtual ICollection<Calificacion> Calificaciones { get; set; } = new List<Calificacion>(); // Añadir esta línea
}