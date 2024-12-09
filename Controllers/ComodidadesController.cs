using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hotel.Models;
using Hotel.Helpers;

namespace Hotel.Controllers
{
    [AuthorizePermission("Comodidades")]
    public class ComodidadesController : Controller
    {
        private readonly HotelContext _context;
        private const int PageSize = 10;

        public ComodidadesController(HotelContext context)
        {
            _context = context;
        }

        // GET: Comodidades
        [AuthorizePermission("Comodidades")]
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var pageNumber = page ?? 1;

            var comodidades = _context.Comodidades.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                comodidades = comodidades.Where(c =>
                    c.Nombre.Contains(searchString) ||
                    c.Descripcion.Contains(searchString));
            }

            var paginatedList = await PaginatedList<Comodidade>.CreateAsync(comodidades.AsNoTracking(), pageNumber, PageSize);

            ViewData["CurrentFilter"] = searchString;
            return View(paginatedList);
        }

        // GET: Comodidades/DetailsPartial/5
        [AuthorizePermission("Comodidades")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comodidade = await _context.Comodidades.FirstOrDefaultAsync(m => m.Id == id);
            if (comodidade == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DetailsComodidades", comodidade);
            }
            return View(comodidade);
        }

        // GET: Comodidades/Create
        [AuthorizePermission("Comodidades")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Comodidades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,Imagen,Activo")] Comodidade comodidade)
        {
            if (ModelState.IsValid)
            {
                comodidade.Id = Guid.NewGuid();
                _context.Add(comodidade);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(comodidade);
        }

        // GET: Comodidades/Edit/5
        [AuthorizePermission("Comodidades")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comodidade = await _context.Comodidades.FindAsync(id);
            if (comodidade == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditComodidades", comodidade);
            }
            return View(comodidade);
        }

        // POST: Comodidades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Comodidades")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nombre,Descripcion,Imagen,Activo")] Comodidade comodidade)
        {
            if (id != comodidade.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comodidade);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, id = comodidade.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComodidadeExists(comodidade.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditComodidades", comodidade);
            }
            return View(comodidade);
        }

        // GET: Comodidades/Delete/5
        [AuthorizePermission("Comodidades")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comodidade = await _context.Comodidades
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comodidade == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DeleteComodidades", comodidade);
            }
            return View(comodidade);
        }

        // POST: Comodidades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Comodidades")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var comodidade = await _context.Comodidades.FindAsync(id);
            if (comodidade != null)
            {
                _context.Comodidades.Remove(comodidade);
                await _context.SaveChangesAsync();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ComodidadeExists(Guid id)
        {
            return _context.Comodidades.Any(e => e.Id == id);
        }
    }
}