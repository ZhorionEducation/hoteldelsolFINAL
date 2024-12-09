using System;
using System.Collections.Generic;

namespace Hotel.Models;

public partial class Role
{
    public Guid Id { get; set; }

    
    public string Nombre { get; set; } = null!;
    public bool Estado { get; set; }
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();

}