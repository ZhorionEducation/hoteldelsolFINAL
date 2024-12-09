using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.ViewModels
{
    public class UsuarioEditViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string NombreUsuario { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "El nombre solo puede contener letras.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "El apellido solo puede contener letras.")]
        public string Apellido { get; set; } = null!;

        [Phone(ErrorMessage = "El número de teléfono no es válido.")]
        public string? Telefono { get; set; }

        public Guid? GeneroId { get; set; }

        public Guid? RolId { get; set; }

        public bool Activo { get; set; }

        // Nuevos campos opcionales
        public long? NumeroDocumento { get; set; }

        public Guid? TipoDocumentoId { get; set; }

        // Select Lists para los dropdowns
        public IEnumerable<SelectListItem>? Generos { get; set; }

        public IEnumerable<SelectListItem>? Roles { get; set; }

        public IEnumerable<SelectListItem>? TiposDocumento { get; set; }
    }
}