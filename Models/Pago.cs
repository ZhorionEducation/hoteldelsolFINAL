using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Models;

public partial class Pago
{
    public Guid Id { get; set; }

    public Guid? ReservaId { get; set; }

    [Required(ErrorMessage = "El metodo de pago es requerido")]
    public string MetodoPago { get; set; } = null!;

    public DateOnly FechaPago { get; set; }

    [Required(ErrorMessage = "El monto que pagaste es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que 0")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "El estado es requerido")]
    public string Estado { get; set; } = null!;

    
    public string? ComprobantePath { get; set; }

    public virtual Reserva? Reserva { get; set; }
}
