// DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using Hotel.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace Hotel.Controllers
{
    [AuthorizePermission("Dashboard")]
    public class DashboardController : Controller
    {
        private readonly HotelContext _context;

        public DashboardController(HotelContext context)
        {
            _context = context;
        }

        [AuthorizePermission("Dashboard")]
        public IActionResult Index(int? month)
        {
            int selectedMonth = month ?? DateTime.Today.Month;

            var hotelRatings = _context.Calificaciones.Select(c => c.CalificacionHotel).ToList();
            var serviceRatings = _context.Calificaciones.Select(c => c.CalificacionServicio).ToList();
            var roomRatings = _context.Calificaciones.Select(c => c.CalificacionHabitacion).ToList();

            var averageHotelRatings = hotelRatings
                .GroupBy(r => r)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Rating, x => x.Count);

            var averageServiceRatings = serviceRatings
                .GroupBy(r => r)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Rating, x => x.Count);

            var averageRoomRatings = roomRatings
        .GroupBy(r => r)
        .Select(g => new { Rating = g.Key, Count = g.Count() })
        .ToDictionary(x => x.Rating, x => x.Count);

            var reservasPorMes = Enumerable.Range(1, 12)
                .ToDictionary(m => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m), m => 0);

            var reservasAgrupadas = _context.Reservas
                .Where(r => r.Pagos.All(p => p.Estado != "Cancelado"))
                .GroupBy(r => r.FechaReserva.HasValue ? r.FechaReserva.Value.Month : 0)
                .Select(g => new { Mes = g.Key, Cantidad = g.Count() })
                .ToList();

            foreach (var reserva in reservasAgrupadas)
            {
                if (reserva.Mes > 0)
                {
                    reservasPorMes[CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(reserva.Mes)] = reserva.Cantidad;
                }
            }

            var mostBookedRooms = _context.Reservas
        .Where(r => r.FechaInicio.Month == selectedMonth &&
                    r.Pagos.All(p => p.Estado != "Cancelado"))
        .GroupBy(r => r.Habitacion.NumeroHabitacion)  // Assuming Habitacion has a Numero property
        .Select(g => new
        {
            RoomNumber = g.Key,
            BookingCount = g.Count()
        })
        .OrderByDescending(x => x.BookingCount)
        .Take(5)  // Get top 5 most booked rooms
        .ToDictionary(x => $"Habitación {x.RoomNumber}", x => x.BookingCount);

            // Room Occupancy by Selected Month
            var reservasEnMes = _context.Reservas
                .Where(r => r.FechaInicio.Month == selectedMonth &&
                            r.Pagos.All(p => p.Estado != "Cancelado"))
                .Select(r => r.HabitacionId)
                .Distinct()
                .Count();

            var totalRooms = _context.Habitaciones.Count();

            var roomOccupancy = new Dictionary<string, double>
    {
        {"Ocupadas", Math.Round((double)reservasEnMes, 2)},
        {"Disponibles", Math.Round((double)(totalRooms - reservasEnMes), 2)}
    };

            // Calcular la habitación mejor calificada
            var bestRatedRoom = _context.Calificaciones
                .GroupBy(c => c.HabitacionId)
                .Select(g => new
                {
                    HabitacionId = g.Key,
                    AverageRating = g.Average(c => c.CalificacionHabitacion)
                })
                .OrderByDescending(x => x.AverageRating)
                .FirstOrDefault();

            string bestRatedRoomNumber = null;
            double bestRatedRoomScore = 0;

            if (bestRatedRoom != null)
            {
                var habitacion = _context.Habitaciones.Find(bestRatedRoom.HabitacionId);
                if (habitacion != null)
                {
                    bestRatedRoomNumber = habitacion.NumeroHabitacion;
                    bestRatedRoomScore = bestRatedRoom.AverageRating;
                }
            }

            var bestRatedRoomByType = _context.Calificaciones
                .GroupBy(c => new { c.Habitacion.TipoHabitacion, c.Habitacion.NumeroHabitacion })
                .Select(g => new BestRatedRoomByTypeModel
                {
                    RoomNumber = g.Key.NumeroHabitacion,
                    AverageRating = g.Average(c => c.CalificacionHabitacion),
                    Tipo = g.Key.TipoHabitacion
                })
                .ToDictionary(x => x.RoomNumber, x => x);

            // Calcular los promedios globales
            double globalHotelRating = hotelRatings.Any() ? hotelRatings.Average() : 0;
            double globalServiceRating = serviceRatings.Any() ? serviceRatings.Average() : 0;
            double globalRoomRating = roomRatings.Any() ? roomRatings.Average() : 0;

            var viewModel = new DashboardViewModel
            {
                HotelRatings = hotelRatings,
                ServiceRatings = serviceRatings,
                AverageHotelRatings = averageHotelRatings,
                AverageServiceRatings = averageServiceRatings,
                MonthlyBookings = reservasPorMes,
                RoomOccupancy = roomOccupancy,
                TotalUsers = _context.Usuarios.Count(u => u.Activo),
                TotalReservations = _context.Reservas.Count(r => r.Pagos.Any(p => p.Estado == "En ejecución")),
                TotalPayments = _context.Pagos.Count(),

                TotalRooms = totalRooms,
                TotalServices = _context.ServiciosAdicionales.Count(),
                SelectedMonth = selectedMonth,
                Months = Enumerable.Range(1, 12)
                                .Select(m => new SelectListItem
                                {
                                    Value = m.ToString(),
                                    Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)
                                }).ToList(),
                GlobalHotelRating = globalHotelRating, // Asignar el promedio global
                GlobalServiceRating = globalServiceRating,
                GlobalRoomRating = globalRoomRating,
                BestRatedRoomNumber = bestRatedRoomNumber,
                BestRatedRoomScore = bestRatedRoomScore,
                MostBookedRooms = mostBookedRooms,
                BestRatedRoomByType = bestRatedRoomByType

            };

            return View(viewModel);
        }
    }
}