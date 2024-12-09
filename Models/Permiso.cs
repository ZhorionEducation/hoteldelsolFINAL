using System;
using System.Collections.Generic;

namespace Hotel.Models;

public partial class Permiso
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; } = true;

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
