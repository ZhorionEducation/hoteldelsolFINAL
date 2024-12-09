using System;
using System.Collections.Generic;

namespace Hotel.Models;

public partial class Municipio
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int? DepartamentoId { get; set; }

    public virtual Departamento? Departamento { get; set; }
}
