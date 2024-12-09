using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hotel.Models;
using Hotel.Helpers;
using Microsoft.Extensions.Hosting;

namespace Hotel.Controllers
{
    [AuthorizePermission("Servicios")]
    public class ServiciosAdicionalesController : Controller
    {
        private readonly HotelContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ServiciosAdicionalesController(HotelContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [AuthorizePermission("Servicios")]
        public IActionResult IndexUsuarios()
        {
            var servicios = _context.ServiciosAdicionales.ToList();
            return View(servicios);
        }

        // GET: ServiciosAdicionales
        [AuthorizePermission("Servicios")]
        public IActionResult IndexUsuarios(string searchString, int? page)
        {
            var servicios = _context.ServiciosAdicionales.ToList();
            return View(servicios);
        }

        public async Task<IActionResult> Index(string searchString, int? page)
        {
            int pageNumber = page ?? 1;
            const int PageSize = 5;

            var servicios = _context.ServiciosAdicionales.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                servicios = servicios.Where(s => s.Nombre.Contains(searchString));
            }

            var paginatedList = await PaginatedList<ServiciosAdicionale>.CreateAsync(servicios.AsNoTracking(), pageNumber, PageSize);

            ViewData["CurrentFilter"] = searchString;
            return View(paginatedList);
        }

        // GET: ServiciosAdicionales/Details/5
        [AuthorizePermission("Servicios")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _context.ServiciosAdicionales
                .FirstOrDefaultAsync(m => m.Id == id.Value);
            if (servicio == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsServicio", servicio);
        }

        // GET: ServiciosAdicionales/Create
        [AuthorizePermission("Servicios")]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Servicios")]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,Precio,Imagen,Activo")] ServiciosAdicionale serviciosAdicionale, IFormFile imagen)
        {
            if (ModelState.IsValid)
            {
                serviciosAdicionale.Id = Guid.NewGuid();

                serviciosAdicionale.Activo = Request.Form["Activo"].ToString().ToLower() == "true";

                if (imagen != null && imagen.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img/servicios");

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

                    serviciosAdicionale.Imagen = uniqueFileName;
                }

                _context.Add(serviciosAdicionale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(serviciosAdicionale);
        }

        // GET: ServiciosAdicionales/Edit/5
        [HttpGet]
        [AuthorizePermission("Servicios")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _context.ServiciosAdicionales.FindAsync(id);
            if (servicio == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditServicio", servicio);
            }
            return View(servicio);
        }

        // POST: ServiciosAdicionales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Servicios")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nombre,Descripcion,Precio,Imagen,Activo")] ServiciosAdicionale servicio, IFormFile? nuevaImagen)
        {
            if (id != servicio.Id)
            {
                return Json(new { success = false, message = "ID no coincide" });
            }

            try
            {
                var servicioExistente = await _context.ServiciosAdicionales
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (servicioExistente == null)
                {
                    return Json(new { success = false, message = "Servicio no encontrado" });
                }

                // Handle image update
                if (nuevaImagen != null && nuevaImagen.Length > 0)
                {
                    // Process new image upload
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(nuevaImagen.FileName);
                    string path = Path.Combine(_webHostEnvironment.WebRootPath, "img", "servicios", fileName);

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(servicioExistente.Imagen))
                    {
                        string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "servicios", servicioExistente.Imagen);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await nuevaImagen.CopyToAsync(stream);
                    }
                    servicio.Imagen = fileName;
                }
                else
                {
                    // Keep existing image if no new image was uploaded
                    servicio.Imagen = servicioExistente.Imagen;
                }

                _context.Update(servicio);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ServiciosAdicionales/Delete/5
        [AuthorizePermission("Servicios")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviciosAdicionale = await _context.ServiciosAdicionales
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviciosAdicionale == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DeleteServicio", serviciosAdicionale);
            }

            return View(serviciosAdicionale);
        }

        // POST: ServiciosAdicionales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Servicios")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var serviciosAdicionale = await _context.ServiciosAdicionales
                    .Include(s => s.Reservas)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (serviciosAdicionale == null)
                {
                    return Json(new { success = false, message = "Servicio no encontrado" });
                }

                // Check if service has reservations
                if (serviciosAdicionale.Reservas != null && serviciosAdicionale.Reservas.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se puede eliminar el servicio porque tiene reservas asociadas"
                    });
                }

                // Delete image if exists
                if (!string.IsNullOrEmpty(serviciosAdicionale.Imagen))
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img/servicios");
                    string imagePath = Path.Combine(uploadsFolder, serviciosAdicionale.Imagen);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.ServiciosAdicionales.Remove(serviciosAdicionale);
                await _context.SaveChangesAsync();
                return Json(new { success = true, id = id });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al eliminar el servicio: " + ex.Message
                });
            }
        }

        private bool ServicioExists(Guid id)
        {
            return _context.ServiciosAdicionales.Any(e => e.Id == id);
        }
    }
}
