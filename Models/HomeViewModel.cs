using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Models;

public class HomeViewModel
{
    public List<Habitacione> Habitaciones { get; set; }
    public List<ServiciosAdicionale> Servicios { get; set; }
}