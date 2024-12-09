using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Models;

public partial class Usuario
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "El nombre de usuario solo puede contener letras y números.")]
    public string NombreUsuario { get; set; } = null!;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    public string CorreoElectronico { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Contrasena { get; set; } = null!;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "El nombre solo puede contener letras.")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "El apellido solo puede contener letras.")]
    public string Apellido { get; set; } = null!;

    

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe tener exactamente 10 dígitos.")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [DataType(DataType.Date)]
    [CustomValidation(typeof(Usuario), nameof(ValidarFechaNacimiento))]
    public DateOnly? FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El género es obligatorio.")]
    public Guid? GeneroId { get; set; }

    public Guid? RolId { get; set; }

    public string? ImagenPerfil { get; set; }

    public DateOnly? FechaRegistro { get; set; }

    public bool Activo { get; set; }

    [Required(ErrorMessage = "El número de documento es obligatorio")]
    [Display(Name = "Número de Documento")]
    [Range(10000000, 9999999999, ErrorMessage = "El número de documento debe tener entre 8 y 10 dígitos")]
    public long? NumeroDocumento { get; set; } // Nuevo campo

    [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
    public Guid TipoDocumentoId { get; set; }

    public virtual Genero? Genero { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual Role? Rol { get; set; }

    public virtual TipoDocumento? TipoDocumento { get; set; } // Nueva relación

    public static ValidationResult? ValidarFechaNacimiento(DateOnly? fechaNacimiento, ValidationContext context)
    {
        if (!fechaNacimiento.HasValue)
            return new ValidationResult("La fecha de nacimiento es obligatoria.");

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var edad = hoy.Year - fechaNacimiento.Value.Year;
        
        if (fechaNacimiento.Value.AddYears(edad) > hoy)
            edad--;

        if (edad < 18)
            return new ValidationResult("Debes ser mayor de 18 años para registrarte.");
        
        if (edad > 123)
            return new ValidationResult("La edad máxima permitida es 123 años.");

        return ValidationResult.Success;
    }

    [NotMapped]
    [MinLength(8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres.")]
    public string? NewPassword { get; set; }
}