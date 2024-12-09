using Hotel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hotel.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using System.Security.Claims;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Diagnostics;
using OfficeOpenXml;
using System.Globalization;

namespace Hotel.Controllers
{
    public class ReservasController : Controller
    {
        private readonly HotelContext _context;
        private const int PageSize = 10; //Cuantos items por pagina
        public ReservasController(HotelContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: Reservas
        [AuthorizePermission("Reservas")]
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var userRole = User.IsInRole("Administrador") ? "Administrador" : "Usuario";
            ViewData["UserRole"] = userRole;

            int pageNumber = page ?? 1;

            var hotelContext = _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Comodidads)
                .Include(r => r.Servicios)
                .Include(r => r.Pagos)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                hotelContext = hotelContext.Where(r =>
                    r.Id.ToString().Contains(searchString) ||
                    (r.Usuario != null && r.Usuario.NombreUsuario.Contains(searchString)) ||
                    (r.Habitacion != null && r.Habitacion.NumeroHabitacion.Contains(searchString)) ||
                    r.Comodidads.Any(c => c.Nombre.Contains(searchString)) ||
                    r.Servicios.Any(s => s.Nombre.Contains(searchString)) ||
                    r.Pagos.Any(p => p.Estado.Contains(searchString))
                );
            }

            var paginatedList = await PaginatedList<Reserva>.CreateAsync(hotelContext.AsNoTracking(), pageNumber, PageSize);

            ViewData["CurrentFilter"] = searchString;
            return View(paginatedList);
        }

        // Tipo habitacion para filtro en el create
        public JsonResult GetHabitacionesByTipo(string tipo)
        {
            var habitaciones = _context.Habitaciones
                .Where(h => h.TipoHabitacion == tipo && h.Activo == true)
                .Select(h => new { h.Id, h.NumeroHabitacion })
                .ToList();

            return Json(habitaciones);
        }
        public IActionResult GetAllHabitaciones()
        {
            var habitaciones = _context.Habitaciones
                .Where(h => h.Activo == true)
                .Select(h => new { id = h.Id, numeroHabitacion = h.NumeroHabitacion })
                .ToList();
            return Json(habitaciones);
        }


        // GET: Reservas/MisReservas
        [AuthorizePermission("Usuarios , ReservasUsuario")]
        public async Task<IActionResult> MisReservas(string searchString, int? page)
        {
            int pageNumber = page ?? 1;

            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Usuarios", "Create");
            }

            var userName = User.Identity.Name;
            var user = await _context.Usuarios.SingleOrDefaultAsync(u => u.NombreUsuario == userName);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }

            var userReservas = _context.Reservas
                .Include(r => r.Habitacion)
                .Include(r => r.Comodidads)
                .Include(r => r.Servicios)
                .Include(r => r.Pagos)
                .Where(r => r.UsuarioId == user.Id);


            if (!string.IsNullOrEmpty(searchString))
            {
                userReservas = userReservas.Where(r =>
                    r.Id.ToString().Contains(searchString) ||
                    r.Usuario.NombreUsuario.Contains(searchString) ||
                    r.Habitacion.NumeroHabitacion.Contains(searchString) ||
                    r.Comodidads.Any(c => c.Nombre.Contains(searchString)) ||
                    r.Servicios.Any(s => s.Nombre.Contains(searchString)) ||
                    r.Pagos.Any(p => p.Estado.Contains(searchString))
                );
            }

            var paginatedList = await PaginatedList<Reserva>.CreateAsync(userReservas.AsNoTracking(), pageNumber, PageSize);


            ViewData["CurrentFilter"] = searchString;
            return View(paginatedList);
        }


        [HttpPost]
        public async Task<IActionResult> ExportReservations(Guid reservationId, string format)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Habitacion)
                .Include(r => r.Comodidads)
                .Include(r => r.Servicios)
                .Include(r => r.Pagos)
                .Include(u => u.Usuario)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reserva == null)
            {
                return NotFound();
            }

            if (format == "pdf")
            {
                // Lógica para exportar a PDF
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == reserva.UsuarioId);
                var pago = reserva.Pagos.FirstOrDefault();
                var serviciosAdicionale = reserva.Servicios.FirstOrDefault();
                return ExportReservationToPdf(reserva, usuario, pago, serviciosAdicionale);
            }
            else if (format == "excel")
            {
                // Lógica para exportar a Excel
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == reserva.UsuarioId);
                var pago = reserva.Pagos.FirstOrDefault();
                var serviciosAdicionale = reserva.Servicios.FirstOrDefault();
                return ExportReservationToExcel(reserva, usuario, pago, serviciosAdicionale);
            }

            return BadRequest("Formato no soportado");
        }

        private IActionResult ExportReservationToPdf(Reserva reserva, Usuario usuario, Pago pago, ServiciosAdicionale serviciosAdicionale)
        {
            if (reserva == null || usuario == null || reserva.Habitacion == null)
            {
                return BadRequest("Datos de la reserva incompletos");
            }

            using (var stream = new MemoryStream())
            {
                var document = new Document();
                PdfWriter.GetInstance(document, stream).CloseStream = false;
                document.Open();

                // Agregar título
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                document.Add(new Paragraph("Detalles de la Reserva", titleFont) { Alignment = Element.ALIGN_CENTER });

                // Agregar espacio
                document.Add(new Paragraph("\n"));

                // Crear tabla
                var table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.AddCell("Reserva ID");
                table.AddCell(reserva.Id.ToString());
                table.AddCell("Usuario que hizo la reserva");
                table.AddCell(usuario.Nombre);
                table.AddCell("Habitación");
                table.AddCell($"{reserva.Habitacion.NumeroHabitacion}, {reserva.Habitacion.Descripcion}");
                table.AddCell("Estado de la reserva");
                table.AddCell(pago == null || string.IsNullOrEmpty(pago.Estado) ? "Sin pagos" : pago.Estado);
                table.AddCell("Fecha Inicio");
                table.AddCell(reserva.FechaInicio.ToString("yyyy-MM-dd"));
                table.AddCell("Fecha Fin");
                table.AddCell(reserva.FechaFin.ToString("yyyy-MM-dd"));
                table.AddCell("Fecha en la que reservo");
                table.AddCell(reserva.FechaReserva.HasValue ? reserva.FechaReserva.Value.ToString("yyyy-MM-dd") : "N/A");
                table.AddCell("Servicios adicionales");
                table.AddCell(serviciosAdicionale == null || string.IsNullOrEmpty(serviciosAdicionale.Nombre) ? "Sin servicios" : serviciosAdicionale.Nombre);
                decimal precioConIva = reserva.PrecioTotal * 1.19m;
                table.AddCell("Precio Total (con IVA 19%)");
                table.AddCell(precioConIva.ToString("N2", new CultureInfo("es-CO")) + " COP");

                document.Add(table);
                document.Close();

                var content = stream.ToArray();
                return File(content, "application/pdf", "Reserva.pdf");
            }
        }

        private IActionResult ExportReservationToExcel(Reserva reserva, Usuario usuario, Pago pago, ServiciosAdicionale serviciosAdicionale)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Reserva");

                // Estilo para el encabezado
                var headerStyle = worksheet.Cells["A1:B1"].Style;
                headerStyle.Font.Bold = true;
                headerStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerStyle.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Agregar datos
                worksheet.Cells["A1"].Value = "Reserva ID";
                worksheet.Cells["B1"].Value = reserva.Id.ToString();
                worksheet.Cells["A2"].Value = "Usuario que hizo la reserva";
                worksheet.Cells["B2"].Value = usuario.Nombre;
                worksheet.Cells["A3"].Value = "Habitación";
                worksheet.Cells["B3"].Value = $"{reserva.Habitacion.NumeroHabitacion}, {reserva.Habitacion.Descripcion}";
                worksheet.Cells["A4"].Value = "Estado de la reserva";
                worksheet.Cells["B4"].Value = pago == null || string.IsNullOrEmpty(pago.Estado) ? "Sin pagos" : pago.Estado;
                worksheet.Cells["A5"].Value = "Fecha Inicio";
                worksheet.Cells["B5"].Value = reserva.FechaInicio.ToString("yyyy-MM-dd");
                worksheet.Cells["A6"].Value = "Fecha Fin";
                worksheet.Cells["B6"].Value = reserva.FechaFin.ToString("yyyy-MM-dd");
                worksheet.Cells["A7"].Value = "Fecha en la que reservo";
                worksheet.Cells["B7"].Value = reserva.FechaReserva.HasValue ? reserva.FechaReserva.Value.ToString("yyyy-MM-dd") : "N/A";
                worksheet.Cells["A8"].Value = "Servicios adicionales";
                worksheet.Cells["B8"].Value = serviciosAdicionale == null || string.IsNullOrEmpty(serviciosAdicionale.Nombre) ? "Sin servicios" : serviciosAdicionale.Nombre;
                decimal precioConIva = reserva.PrecioTotal * 1.19m;
                worksheet.Cells["A9"].Value = "Precio Total (con IVA 19%)";
                worksheet.Cells["B9"].Value = precioConIva.ToString("N2", new CultureInfo("es-CO")) + " COP";

                // Aplicar bordes a todas las celdas
                using (var range = worksheet.Cells["A1:B9"])
                {
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reserva.xlsx");
            }
        }


        [HttpGet]
        public IActionResult CheckNotifications()
        {
            try
            {
                var userId = User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { hasNotification = false, message = "Usuario no autenticado" });
                }

                if (!Guid.TryParse(userId, out Guid parsedUserId))
                {
                    return Json(new { hasNotification = false, message = "ID de usuario inválido" });
                }

                var reservasSinCalificar = _context.Reservas
                    .Include(r => r.Pagos)
                    .Include(r => r.Calificaciones) // Incluir calificaciones
                    .Where(r => r.UsuarioId == parsedUserId
                        && r.Pagos != null
                        && r.Pagos.Any(p => p != null && p.Estado == "Finalizado")
                        && !r.Calificaciones.Any()) // Filtrar las que no tienen calificaciones
                    .Select(r => new
                    {
                        id = r.Id,
                        fechaInicio = r.FechaInicio != null ? r.FechaInicio.ToString("yyyy-MM-dd") : "",
                        fechaFin = r.FechaFin != null ? r.FechaFin.ToString("yyyy-MM-dd") : ""
                    })
                    .ToList();

                if (!reservasSinCalificar.Any())
                {
                    return Json(new { hasNotification = false, message = "No hay reservas pendientes por calificar" });
                }

                return Json(new { hasNotification = true, reservas = reservasSinCalificar });
            }
            catch (Exception ex)
            {
                return Json(new { hasNotification = false, message = "Error al verificar notificaciones" });
            }
        }

        [HttpPost]
        public IActionResult SubmitRating([FromBody] RatingModel model)
        {
            if (model == null)
            {
                return BadRequest("Modelo de calificación es nulo.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return BadRequest("Datos de calificación inválidos: " + string.Join(", ", errors));
            }

            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest("UserId no es válido.");
            }

            var reserva = _context.Reservas
                .Include(r => r.Pagos)
                .FirstOrDefault(r => r.Id == model.ReservaId && r.UsuarioId == userId);

            if (reserva == null)
            {
                return BadRequest("Reserva no encontrada.");
            }

            var existingCalificacion = _context.Calificaciones
                .FirstOrDefault(c => c.ReservaId == model.ReservaId);

            if (existingCalificacion != null)
            {
                return BadRequest("Ya has calificado esta reserva.");
            }

            var calificacion = new Calificacion
            {
                ReservaId = model.ReservaId,
                CalificacionServicio = model.CalificacionServicio,
                CalificacionHotel = model.CalificacionHotel,
                CalificacionHabitacion = model.CalificacionHabitacion,
                HabitacionId = reserva.HabitacionId ?? Guid.Empty
            };

            _context.Calificaciones.Add(calificacion);
            _context.SaveChanges();

            return Ok("Calificación enviada exitosamente.");
        }

        public class RatingModel
        {
            public Guid ReservaId { get; set; }
            public int CalificacionServicio { get; set; }

            public int CalificacionHotel { get; set; }
            public int CalificacionHabitacion { get; set; }
        }

        public async Task<IActionResult> Hospedaje()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Habitacion)
                .Include(r => r.Pagos)
                .Where(r => r.Pagos.Any(p => p.Estado == "En Ejecucion"))
                .ToListAsync();

            return View(reservas);
        }

        // GET: Reservas/Details/5
        [AuthorizePermission("Reservas , ReservasUsuario")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Habitacion)
                .Include(r => r.Usuario)
                .Include(r => r.Comodidads)
                .Include(r => r.Servicios)
                .Include(r => r.Huespedes)
                .Include(r => r.Pagos)
                .Include(r => r.Habitacion)
                .ThenInclude(h => h.Comodidades)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DetailsReserva", reserva);
            }

            return View(reserva);
        }

        // GET: Reservas/Create
        public IActionResult Create()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reserva = new Reserva
            {
                Comodidads = new List<Comodidade>(),
                Servicios = new List<ServiciosAdicionale>(),
                Huespedes = new List<Huesped>(),
                Pagos = new List<Pago>(),
                Calificaciones = new List<Calificacion>()
            };

            var habitaciones = _context.Habitaciones
                .Select(h => new HabitacionSelectListItem
                {
                    Value = h.Id.ToString(),
                    Text = h.NumeroHabitacion,
                    Activo = h.Activo ?? false
                })
                .ToList();

            var tiposHabitacion = _context.Habitaciones
                .Where(h => h.Activo == true)
                .Select(h => h.TipoHabitacion)
                .Distinct()
                .ToList();

            ViewBag.HabitacionId = habitaciones ?? new List<HabitacionSelectListItem>();
            ViewBag.TiposHabitacion = new SelectList(tiposHabitacion ?? new List<string?>());
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios.Where(u => u.Activo == true).ToList() ?? new List<Usuario>(), "Id", "NombreUsuario", currentUserId);
            ViewData["ComodidadId"] = new SelectList(_context.Comodidades.ToList() ?? new List<Comodidade>(), "Id", "Nombre");
            ViewBag.ServiciosAdicionales = _context.ServiciosAdicionales
                .Select(s => new
                {
                    s.Id,
                    s.Nombre,
                    s.Precio,
                    s.Activo
                })
                .ToList();

            return View();
        }

        // POST: Reservas/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UsuarioId,HabitacionId,FechaInicio,FechaFin,PrecioTotal,NumeroAcompanantes,FechaReserva,Comodidads,Servicios,Calificaciones")] Reserva reserva, List<Guid> Comodidads, List<Guid> Servicios, List<Huesped> Huespedes, string submitButton)
        {
            if (ModelState.IsValid)
            {
                reserva.Calificaciones = new List<Calificacion>();
                if (_context.Reservas.Any(r => r.Id == reserva.Id))
                {
                    return Json(new { success = false, message = "La reserva ya existe." });
                }

                reserva.Id = Guid.NewGuid();
                foreach (var ComodidadId in Comodidads)
                {
                    var comodidad = await _context.Comodidades.FindAsync(ComodidadId);
                    if (comodidad != null)
                    {
                        reserva.Comodidads.Add(comodidad);
                    }
                }

                foreach (var ServicioAdicionalId in Servicios)
                {
                    var servicio = await _context.ServiciosAdicionales.FindAsync(ServicioAdicionalId);
                    if (servicio != null)
                    {
                        reserva.Servicios.Add(servicio);
                    }
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId != null)
                {
                    // Buscar el usuario en la base de datos utilizando el UserId
                    var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

                    if (usuario != null)
                    {
                        // Crear un nuevo objeto Huesped para el usuario que hace la reserva
                        var usuarioHuesped = new Huesped
                        {
                            Id = Guid.NewGuid(),
                            ReservaId = reserva.Id,
                            Nombre = usuario.Nombre,
                            DocumentoIdentidad = usuario.NumeroDocumento.ToString() // Asumiendo que NumeroDocumento es el DocumentoIdentidad
                        };

                        // Agregar el usuario que hace la reserva a la lista de Huespedes
                        // reserva.Huespedes.Add(usuarioHuesped);
                    }
                }

                foreach (var huesped in Huespedes)
                {
                    huesped.Id = Guid.NewGuid();
                    huesped.ReservaId = reserva.Id;
                    reserva.Huespedes.Add(huesped);
                }


                _context.Add(reserva);
                await _context.SaveChangesAsync();
                if (submitButton == "Pagar")
                {
                    return Json(new
                    {
                        success = true,
                        pagarUrl = Url.Action("Pagar", "Reservas", new { id = reserva.Id })
                    });
                }
                else if (submitButton == "Guardar")
                {
                    // Redirigir a una vista diferente, por ejemplo, a la lista de reservas
                    string redirectUrl = User.IsInRole("Administrador")
                        ? Url.Action("Index", "Reservas")
                        : Url.Action("MisReservas", "Reservas");

                    return Json(new { success = true, redirectUrl = redirectUrl });
                }
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Si hay un error, se deben recargar las listas de selección
            ViewData["HabitacionId"] = new SelectList(_context.Habitaciones, "Id", "NumeroHabitacion", reserva.HabitacionId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "NombreUsuario", reserva.UsuarioId, currentUserId);
            ViewData["ComodidadId"] = new SelectList(_context.Comodidades, "Id", "Nombre", reserva.Comodidads.Select(c => c.Id));
            ViewData["ServicioAdicionalId"] = new SelectList(_context.ServiciosAdicionales, "Id", "Nombre", reserva.Servicios.Select(s => s.Id));

            return View(reserva);
        }


        // GET: Reservas/Edit/5
        [AuthorizePermission("Usuarios , ReservasUsuario")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Comodidads)
                .Include(r => r.Servicios)
                .Include(r => r.Huespedes)
                .Include(r => r.Habitacion.Comodidades)
                .Include(r => r.Pagos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reserva == null)
            {
                return NotFound();
            }

            // Get the room type and max guests
            var habitacion = await _context.Habitaciones
                .FirstOrDefaultAsync(h => h.Id == reserva.HabitacionId);
            var tipoHabitacion = habitacion?.TipoHabitacion;
            var maxHuespedes = tipoHabitacion switch
            {
                "Individual" => 1,
                "Doble" => 2,
                "Deluxe" => 5,
                _ => 0
            };

            if (User.IsInRole("Administrador"))
            {
                maxHuespedes += 1;
            }

            // Pass the room type and max guests to the view
            ViewBag.TipoHabitacion = tipoHabitacion;
            ViewBag.MaxHuespedes = maxHuespedes;

            ViewData["HabitacionId"] = new SelectList(_context.Habitaciones, "Id", "NumeroHabitacion", reserva.HabitacionId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "NombreUsuario", reserva.UsuarioId);
            ViewData["ComodidadId"] = new MultiSelectList(_context.Comodidades, "Id", "Nombre", reserva.Comodidads.Select(c => c.Id));
            ViewData["ServicioAdicionalId"] = new MultiSelectList(_context.ServiciosAdicionales, "Id", "Nombre", reserva.Servicios.Select(s => s.Id));


            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditReserva", reserva);
            }
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        [AuthorizePermission("Usuarios , ReservasUsuario")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,UsuarioId,HabitacionId,FechaInicio,FechaFin,PrecioTotal,NumeroAcompanantes,FechaReserva")] Reserva reserva, List<Guid> Comodidads, List<Guid> Servicios, List<Huesped> Huespedes)
        {
            if (id != reserva.Id)
            {
                return Json(new { success = false, message = "ID desajustado" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var reservaToUpdate = await _context.Reservas
                        .Include(r => r.Comodidads)
                        .Include(r => r.Servicios)
                        .Include(r => r.Huespedes)
                        .FirstOrDefaultAsync(r => r.Id == id);

                    if (reservaToUpdate == null)
                    {
                        return Json(new { success = false, message = "Reserva no encontrada" });
                    }

                    _context.Entry(reservaToUpdate).CurrentValues.SetValues(reserva);

                    // No modificar el valor de PrecioTotal aquí

                    // Actualizar Comodidades
                    reservaToUpdate.Comodidads = new List<Comodidade>();
                    if (Comodidads != null)
                    {
                        foreach (var comodidadId in Comodidads)
                        {
                            var comodidad = await _context.Comodidades.FindAsync(comodidadId);
                            if (comodidad != null)
                            {
                                reservaToUpdate.Comodidads.Add(comodidad);
                            }
                        }
                    }

                    // Actualizar Servicios
                    reservaToUpdate.Servicios = new List<ServiciosAdicionale>();
                    if (Servicios != null)
                    {
                        foreach (var servicioId in Servicios)
                        {
                            var servicio = await _context.ServiciosAdicionales.FindAsync(servicioId);
                            if (servicio != null)
                            {
                                reservaToUpdate.Servicios.Add(servicio);
                            }
                        }
                    }

                    // Actualizar Huespedes
                    var existingHuespedes = _context.Huespedes.Where(h => h.ReservaId == reserva.Id).ToList();

                    // Eliminar huespedes eliminados
                    foreach (var existingHuesped in existingHuespedes)
                    {
                        if (!Huespedes.Any(h => h.Id == existingHuesped.Id))
                        {
                            _context.Huespedes.Remove(existingHuesped);
                        }
                    }

                    // Agregar o actualizar huespedes
                    foreach (var huesped in Huespedes)
                    {
                        var existingHuesped = existingHuespedes.FirstOrDefault(h => h.Id == huesped.Id);
                        if (existingHuesped != null)
                        {
                            // Actualizar huesped existente
                            existingHuesped.Nombre = huesped.Nombre;
                            existingHuesped.DocumentoIdentidad = huesped.DocumentoIdentidad;
                            _context.Huespedes.Update(existingHuesped);
                        }
                        else
                        {
                            // Agregar nuevo huesped
                            huesped.Id = Guid.NewGuid();
                            huesped.ReservaId = reserva.Id;
                            _context.Huespedes.Add(huesped);
                        }
                    }

                    // Sincronizar NumeroAcompanantes con el número de Huespedes
                    reservaToUpdate.NumeroAcompanantes = Huespedes.Count;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, id = reserva.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.Id))
                    {
                        return Json(new { success = false, message = "Reserva not found" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Concurrency error" });
                    }
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewData["HabitacionId"] = new SelectList(_context.Habitaciones, "Id", "NumeroHabitacion", reserva.HabitacionId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "NombreUsuario", reserva.UsuarioId);
            ViewData["ComodidadId"] = new MultiSelectList(_context.Comodidades, "Id", "Nombre", Comodidads);
            ViewData["ServicioAdicionalId"] = new MultiSelectList(_context.ServiciosAdicionales, "Id", "Nombre", Servicios);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditReserva", reserva);
            }
            return View(reserva);
        }

        //Fechas ocupadas por habitacion
        [HttpGet]
        public async Task<IActionResult> GetFechasOcupadas(Guid habitacionId)
        {
            var fechasOcupadas = await _context.Reservas
                .GroupJoin(_context.Pagos,
                    reserva => reserva.Id,
                    pago => pago.ReservaId,
                    (reserva, pagos) => new { Reserva = reserva, Pagos = pagos })
                .SelectMany(x => x.Pagos.DefaultIfEmpty(),
                    (reserva, pago) => new { Reserva = reserva.Reserva, Pago = pago })
                .Where(rp => rp.Reserva.HabitacionId == habitacionId &&
                    (rp.Pago == null || (rp.Pago.Estado != "Cancelado" && rp.Pago.Estado != "Finalizado")))
                .Select(rp => new
                {
                    fechaInicio = rp.Reserva.FechaInicio,
                    fechaFin = rp.Reserva.FechaFin,
                    reservaId = rp.Reserva.Id
                })
                .ToListAsync();

            return Json(fechasOcupadas);
        }


        // Obtener precio de la reserva al reservar
        [HttpGet]
        public IActionResult GetPrecioHabitacion(Guid id)
        {
            var habitacion = _context.Habitaciones
                .Where(h => h.Id == id)
                .Select(h => h.PrecioPorNoche)
                .FirstOrDefault();
            if (habitacion != 0)
            {
                return Json(habitacion);
            }
            return Json(0);
        }

        [HttpGet]
        public IActionResult GetPrecioServicio(Guid id)
        {
            var servicio = _context.ServiciosAdicionales
                .Where(s => s.Id == id)
                .Select(s => s.Precio)
                .FirstOrDefault();
            if (servicio != 0)
            {
                return Json(servicio);
            }
            return Json(0);
        }


        [HttpGet]
        public IActionResult GetPrecioComodidad(Guid id)
        {
            var comodidad = _context.Comodidades
                .Where(c => c.Id == id)
                .Select(c => c.Precio)
                .FirstOrDefault();
            if (comodidad != 0)
            {
                return Json(comodidad);
            }
            return Json(0);
        }


        [HttpPost]
        public IActionResult CancelarReserva(Guid id)
        {
            try
            {
                var reserva = _context.Reservas
                    .Include(r => r.Pagos)
                    .FirstOrDefault(r => r.Id == id);

                if (reserva == null)
                {
                    return Json(new { success = false, message = "Reserva no encontrada" });
                }

                var pago = new Pago
                {
                    Id = Guid.NewGuid(),
                    ReservaId = reserva.Id,
                    MetodoPago = "Cancelación",
                    FechaPago = DateOnly.FromDateTime(DateTime.Now),
                    Monto = 0,
                    Estado = "Cancelado",
                    ComprobantePath = "N/A"
                };

                _context.Pagos.Add(pago);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cancelar la reserva: " + ex.Message });
            }
        }

        //accion para pagar la reserva
        public async Task<IActionResult> Pagar(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Pagos)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (reserva == null)
            {
                return NotFound();
            }

            var pago = new Pago
            {
                ReservaId = reserva.Id,
                FechaPago = DateOnly.FromDateTime(DateTime.Now),
                Reserva = reserva

            };

            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pagar([Bind("ReservaId,MetodoPago,FechaPago,Monto,Estado")] Pago pago, IFormFile comprobante)
        {
            if (ModelState.IsValid)
            {
                pago.Id = Guid.NewGuid();
                pago.FechaPago = DateOnly.FromDateTime(DateTime.Now);
                pago.Estado = "Por confirmar";

                // Guardar el comprobante en el servidor
                if (comprobante != null && comprobante.Length > 0)
                {
                    var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "comprobantes");
                    Directory.CreateDirectory(directoryPath); // Crear el directorio si no existe

                    var fileExtension = Path.GetExtension(comprobante.FileName);
                    var fileName = $"{pago.Id}{fileExtension}";
                    var filePath = Path.Combine(directoryPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await comprobante.CopyToAsync(stream);
                    }

                    // Guarda la ruta relativa para su uso posterior
                    pago.ComprobantePath = $"/img/comprobantes/{fileName}";
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Debe subir un comprobante." });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Debe subir un comprobante.");
                        return View(pago);
                    }
                }

                _context.Add(pago);
                await _context.SaveChangesAsync();

                // Generar la URL de redirección
                var redirectUrl = User.IsInRole("Administrador")
                    ? Url.Action("Index", "Reservas")
                    : Url.Action("MisReservas", "Reservas");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, redirectUrl = redirectUrl });
                }
                else
                {
                    return User.IsInRole("Administrador")
                        ? RedirectToAction(nameof(Index))
                        : RedirectToAction(nameof(MisReservas));
                }
            }

            return View(pago);
        }


        public async Task<IActionResult> GetPaymentDetails(Guid id)
        {
            var pago = await _context.Pagos
                .Include(p => p.Reserva) // Incluir la Reserva relacionada
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pago == null)
            {
                return NotFound();
            }
            return PartialView("_PaymentDetailsModal", pago);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePaymentDetails([FromForm] PaymentStatusUpdate model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage))
                    });
                }

                var existingPago = await _context.Pagos.FindAsync(model.Id);
                if (existingPago == null)
                {
                    return Json(new { success = false, message = "Pago no encontrado" });
                }

                // Actualizar solo el estado
                existingPago.Estado = model.Estado;

                // Marcar solo la propiedad Estado como modificada
                _context.Entry(existingPago).Property(p => p.Estado).IsModified = true;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Opcional: Loguear el error ex
                return Json(new { success = false, message = "Error al actualizar el estado del pago." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetComodidadesByHabitacion(Guid habitacionId)
        {
            var habitacion = await _context.Habitaciones
                .Include(h => h.Comodidades)
                .FirstOrDefaultAsync(h => h.Id == habitacionId);

            if (habitacion == null)
            {
                return NotFound();
            }

            var comodidades = habitacion.Comodidades.Select(c => c.Id).ToList();
            return Json(comodidades);
        }

        public JsonResult GetHabitacionDetails(Guid id)
        {
            var habitacion = _context.Habitaciones
                .Where(h => h.Id == id)
                .Select(h => new { h.Id, h.NumeroHabitacion, h.TipoHabitacion })
                .FirstOrDefault();

            if (habitacion == null)
            {
                return Json(new { success = false, message = "Habitación no encontrada" });
            }

            return Json(new { success = true, tipoHabitacion = habitacion.TipoHabitacion });
        }




        // GET: Reservas/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Habitacion)
                .Include(r => r.Usuario)
                .Include(r => r.Comodidads)
                .Include(r => r.Servicios)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reserva == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DeleteReserva", reserva);
            }

            return View(reserva);
        }




        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Servicios)
                .Include(r => r.Comodidads)
                .Include(r => r.Pagos)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva != null)
            {
                // Eliminar los pagos asociados
                foreach (var pago in reserva.Pagos.ToList())
                {
                    _context.Pagos.Remove(pago);
                }

                // Eliminar las relaciones entre la reserva y servicios adicionales
                reserva.Servicios.Clear();

                // Eliminar las relaciones entre la reserva y comodidades
                reserva.Comodidads.Clear();

                // Eliminar la reserva
                _context.Reservas.Remove(reserva);

                await _context.SaveChangesAsync();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ReservaExists(Guid id)
        {
            return _context.Reservas.Any(e => e.Id == id);
        }
    }
}
