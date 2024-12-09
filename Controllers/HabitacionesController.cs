using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hotel.Models;
using Microsoft.Extensions.Hosting;
using Hotel.Helpers;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;


namespace Hotel.Controllers
{
    [AuthorizePermission("Habitaciones")]
    public class HabitacionesController : Controller
    {
        private readonly HotelContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public HabitacionesController(HotelContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }




        [AuthorizePermission("Habitaciones")]
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var pageNumber = page ?? 1;
            const int PageSize = 10;

            var habitaciones = _context.Habitaciones
                .Include(h => h.Comodidades)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                habitaciones = habitaciones.Where(h =>
                    h.NumeroHabitacion.Contains(searchString) ||
                    h.Descripcion.Contains(searchString) ||
                    h.TipoHabitacion.Contains(searchString) ||
                    h.Comodidades.Any(c => c.Nombre.Contains(searchString))
                );
            }

            ViewData["CurrentFilter"] = searchString;
            return View(await PaginatedList<Habitacione>.CreateAsync(habitaciones, pageNumber, PageSize));
        }

        public async Task<IActionResult> ExportToPdf(string filters = "all")
        {
            var query = _context.Habitaciones
                .Include(h => h.Reservas)
                .AsQueryable();

            if (filters != "all")
            {
                var filterArray = filters.Split(',');

                // Date filters
                var dateFrom = filterArray.FirstOrDefault(f => f.StartsWith("dateFrom="))?.Split('=')[1];
                var dateTo = filterArray.FirstOrDefault(f => f.StartsWith("dateTo="))?.Split('=')[1];

                if (dateFrom != null && dateTo != null)
                {
                    var fromDate = DateOnly.FromDateTime(DateTime.Parse(dateFrom));
                    var toDate = DateOnly.FromDateTime(DateTime.Parse(dateTo));

                    query = query.Where(h => h.Reservas.Any(r =>
                        (r.FechaInicio >= fromDate && r.FechaInicio <= toDate) ||
                        (r.FechaFin >= fromDate && r.FechaFin <= toDate) ||
                        (r.FechaInicio <= fromDate && r.FechaFin >= toDate)
                    ));
                }

                // Estado filters
                var estadoFilters = new[] { "active", "inactive" }.Intersect(filterArray).ToList();
                if (estadoFilters.Any())
                {
                    query = query.Where(h =>
                        (estadoFilters.Contains("active") && h.Activo == true) ||
                        (estadoFilters.Contains("inactive") && h.Activo == false)
                    );
                }

                // Tipo filters
                var tipoFilters = new[] { "simple", "doble", "suite" }.Intersect(filterArray).ToList();
                if (tipoFilters.Any())
                {
                    query = query.Where(h =>
                        (tipoFilters.Contains("simple") && h.TipoHabitacion == "Individual") ||
                        (tipoFilters.Contains("doble") && h.TipoHabitacion == "Doble") ||
                        (tipoFilters.Contains("suite") && h.TipoHabitacion == "Deluxe")
                    );
                }
            }

            var habitaciones = await query.ToListAsync();

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);

                document.Open();

                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Listado de Habitaciones", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;

                string[] headers = { "Número", "Descripción", "Capacidad", "Precio por Noche" };
                foreach (string header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
                    cell.BackgroundColor = new BaseColor(240, 240, 240);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    table.AddCell(cell);
                }

                foreach (var habitacion in habitaciones)
                {
                    table.AddCell(new PdfPCell(new Phrase(habitacion.NumeroHabitacion.ToString())));
                    table.AddCell(new PdfPCell(new Phrase(habitacion.Descripcion)));
                    table.AddCell(new PdfPCell(new Phrase(habitacion.Capacidad.ToString())));
                    table.AddCell(new PdfPCell(new Phrase($"${habitacion.PrecioPorNoche:N2}")));
                }

                document.Add(table);
                document.Close();

                var fileName = $"Habitaciones_{filters}.pdf";
                return File(ms.ToArray(), "application/pdf", fileName);
            }
        }

        public async Task<IActionResult> ExportToExcel(string filters = "all")
        {
            var query = _context.Habitaciones.AsQueryable();

            if (filters != "all")
            {
                var filterArray = filters.Split(',');

                // Date filters
                var dateFrom = filterArray.FirstOrDefault(f => f.StartsWith("dateFrom="))?.Split('=')[1];
                var dateTo = filterArray.FirstOrDefault(f => f.StartsWith("dateTo="))?.Split('=')[1];

                if (dateFrom != null && dateTo != null)
                {
                    var fromDate = DateOnly.FromDateTime(DateTime.Parse(dateFrom));
                    var toDate = DateOnly.FromDateTime(DateTime.Parse(dateTo));

                    query = query.Where(h => h.Reservas.Any(r =>
                        (r.FechaInicio >= fromDate && r.FechaInicio <= toDate) ||
                        (r.FechaFin >= fromDate && r.FechaFin <= toDate) ||
                        (r.FechaInicio <= fromDate && r.FechaFin >= toDate)
                    ));
                }

                // Estado filters
                var estadoFilters = new[] { "active", "inactive" }.Intersect(filterArray).ToList();
                if (estadoFilters.Any())
                {
                    query = query.Where(h =>
                        (estadoFilters.Contains("active") && h.Activo == true) ||
                        (estadoFilters.Contains("inactive") && h.Activo == false)
                    );
                }

                // Tipo filters
                var tipoFilters = new[] { "simple", "doble", "suite" }.Intersect(filterArray).ToList();
                if (tipoFilters.Any())
                {
                    query = query.Where(h =>
                        (tipoFilters.Contains("simple") && h.TipoHabitacion == "Individual") ||
                        (tipoFilters.Contains("doble") && h.TipoHabitacion == "Doble") ||
                        (tipoFilters.Contains("suite") && h.TipoHabitacion == "Deluxe")
                    );
                }
            }

            var habitaciones = await query.ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheetName = filters switch
                {
                    "active" => "Habitaciones Activas",
                    "inactive" => "Habitaciones Inactivas",
                    "simple" => "Habitaciones Simples",
                    "doble" => "Habitaciones Dobles",
                    "suite" => "Suites",
                    _ => "Todas las Habitaciones"
                };

                var worksheet = package.Workbook.Worksheets.Add(worksheetName);

                worksheet.Cells[1, 1].Value = "Número";
                worksheet.Cells[1, 2].Value = "Descripción";
                worksheet.Cells[1, 3].Value = "Capacidad";
                worksheet.Cells[1, 4].Value = "Precio por Noche";
                worksheet.Cells[1, 5].Value = "Estado";

                var headerRange = worksheet.Cells[1, 1, 1, 5];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                int row = 2;
                foreach (var habitacion in habitaciones)
                {
                    worksheet.Cells[row, 1].Value = habitacion.NumeroHabitacion;
                    worksheet.Cells[row, 2].Value = habitacion.Descripcion;
                    worksheet.Cells[row, 3].Value = habitacion.Capacidad;
                    worksheet.Cells[row, 4].Value = habitacion.PrecioPorNoche;
                    worksheet.Cells[row, 5].Value = habitacion.Activo.HasValue && habitacion.Activo.Value ? "Activo" : "Inactivo";

                    worksheet.Cells[row, 4].Style.Numberformat.Format = "$#,##0.00";

                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                var fileName = $"Habitaciones_{filters}.xlsx";
                return File(package.GetAsByteArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
        }

        // GET: Habitaciones/Details/5
        [AuthorizePermission("Habitaciones")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habitacione = await _context.Habitaciones
                .Include(h => h.Comodidades)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (habitacione == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DetailsHabitacion", habitacione);
            }
            return View(habitacione);
        }

        // GET: Habitaciones/Create
        [AuthorizePermission("Habitaciones")]
        public IActionResult Create()
        {
            ViewData["Comodidades"] = new SelectList(_context.Comodidades, "Id", "Nombre");
            return View();
        }

        // POST: Habitaciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Habitaciones")]
        public async Task<IActionResult> Create([Bind("Id,NumeroHabitacion,Descripcion,Capacidad,PrecioPorNoche,Activo,TipoHabitacion")] Habitacione habitacione, IFormFile imagen, List<Guid> Comodidades)
        {
            if (await _context.Habitaciones.AnyAsync(h => h.NumeroHabitacion == habitacione.NumeroHabitacion))
            {
                return Json(new { success = false, message = "El número de habitación ya está en uso." });
            }

            if (ModelState.IsValid)
            {
                habitacione.Id = Guid.NewGuid();

                if (imagen != null && imagen.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "img/habitaciones");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imagen.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(fileStream);
                    }

                    habitacione.Imagen = uniqueFileName;
                }

                // Manejo de las comodidades
                if (Comodidades != null && Comodidades.Any())
                {
                    foreach (var comodidadId in Comodidades)
                    {
                        var comodidad = await _context.Comodidades.FindAsync(comodidadId);
                        if (comodidad != null)
                        {
                            habitacione.Comodidades.Add(comodidad);
                        }
                    }
                }

                _context.Add(habitacione);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Comodidades"] = new SelectList(_context.Comodidades, "Id", "Nombre", Comodidades);
            return View(habitacione);
        }

        // GET: Habitaciones/Edit/5
        [AuthorizePermission("Habitaciones")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habitacione = await _context.Habitaciones
                .Include(h => h.Comodidades)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (habitacione == null)
            {
                return NotFound();
            }

            // Obtener todas las comodidades
            var todasLasComodidades = await _context.Comodidades.ToListAsync();

            // Obtener los IDs de las comodidades seleccionadas
            var comodidadesSeleccionadas = habitacione.Comodidades
                .Select(c => c.Id)
                .ToList();

            // Crear MultiSelectList
            ViewBag.Comodidades = new MultiSelectList(
                todasLasComodidades,
                "Id",
                "Nombre",
                comodidadesSeleccionadas
            );
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {

                return PartialView("_EditHabitacion", habitacione);
            }
            return View(habitacione);
        }


        // POST: Habitaciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Habitaciones")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,NumeroHabitacion,Descripcion,Capacidad,PrecioPorNoche,Activo,Imagen")] Habitacione habitacione, IFormFile? nuevaImagen, List<Guid> Comodidades)
        {
            if (id != habitacione.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingHabitacione = await _context.Habitaciones
                        .Include(h => h.Comodidades)
                        .FirstOrDefaultAsync(h => h.Id == id);

                    if (existingHabitacione == null)
                    {
                        return NotFound();
                    }

                    // Mantener la imagen existente si no se proporciona una nueva
                    if (nuevaImagen != null && nuevaImagen.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "img/habitaciones");

                        if (!string.IsNullOrEmpty(existingHabitacione.Imagen))
                        {
                            string oldImagePath = Path.Combine(uploadsFolder, existingHabitacione.Imagen);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + nuevaImagen.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await nuevaImagen.CopyToAsync(fileStream);
                        }

                        existingHabitacione.Imagen = uniqueFileName;
                    }
                    else
                    {
                        // Mantener la imagen existente
                        existingHabitacione.Imagen = habitacione.Imagen;
                    }

                    existingHabitacione.NumeroHabitacion = habitacione.NumeroHabitacion;
                    existingHabitacione.Descripcion = habitacione.Descripcion;
                    existingHabitacione.Capacidad = habitacione.Capacidad;
                    existingHabitacione.PrecioPorNoche = habitacione.PrecioPorNoche;
                    existingHabitacione.Activo = habitacione.Activo;

                    existingHabitacione.Comodidades.Clear();
                    if (Comodidades != null && Comodidades.Any())
                    {
                        foreach (var comodidadId in Comodidades)
                        {
                            var comodidad = await _context.Comodidades.FindAsync(comodidadId);
                            if (comodidad != null)
                            {
                                existingHabitacione.Comodidades.Add(comodidad);
                            }
                        }
                    }

                    _context.Update(existingHabitacione);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, id = habitacione.Id });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            return Json(new { success = false, message = "Modelo inválido" });
        }



        // GET: Habitaciones/Delete/5
        [AuthorizePermission("Habitaciones")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habitacione = await _context.Habitaciones
                .FirstOrDefaultAsync(m => m.Id == id);
            if (habitacione == null)
            {
                return NotFound();
            }

            return View(habitacione);
        }

        // POST: Habitaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Habitaciones")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var habitacione = await _context.Habitaciones.FindAsync(id);
            if (habitacione != null)
            {
                _context.Habitaciones.Remove(habitacione);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HabitacioneExists(Guid id)
        {
            return _context.Habitaciones.Any(e => e.Id == id);
        }
    }
}
