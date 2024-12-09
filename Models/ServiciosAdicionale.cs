using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Models;

public partial class ServiciosAdicionale
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser un número positivo.")]
    [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "El precio debe ser un número válido.")]
    public decimal Precio { get; set; }

    public string? Imagen { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
