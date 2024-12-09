using System;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Models
{
    public class PaymentStatusUpdate
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        public string Estado { get; set; } = null!;
    }
}