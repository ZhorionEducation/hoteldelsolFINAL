using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Models
{
    public partial class Comodidade
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string? Descripcion { get; set; }

        public string? Imagen { get; set; }

        public bool? Activo { get; set; }

        public decimal Precio { get; set; }

        public virtual ICollection<Habitacione> Habitaciones { get; set; } = new List<Habitacione>(); // Nueva relación
    }
}