using Hotel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Hotel.Models
{

    public class Calificacion
    {
        public Guid Id { get; set; }
        public Guid ReservaId { get; set; }
        public int CalificacionServicio { get; set; }
        public int CalificacionHotel { get; set; }
        public int CalificacionHabitacion { get; set; }
        public DateTime FechaCalificacion { get; set; }

        public virtual Reserva Reserva { get; set; }
        public Guid HabitacionId { get; set; } // Nueva propiedad
        public Habitacione Habitacion { get; set; } // Nueva propiedad
    }
}