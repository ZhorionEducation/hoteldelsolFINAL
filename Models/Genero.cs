using System;
using System.Collections.Generic;

namespace Hotel.Models;

public partial class Genero
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
