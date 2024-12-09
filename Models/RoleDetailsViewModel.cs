using System;
using System.Collections.Generic;

namespace Hotel.ViewModels
{
    public class RoleDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public bool Estado { get; set; }
        public int CantidadUsuarios { get; set; }
        public List<string> NombresUsuarios { get; set; }
    }
}