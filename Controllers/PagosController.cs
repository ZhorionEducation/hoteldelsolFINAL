using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hotel.Models;

namespace Hotel.Controllers
{
    public class PagosController : Controller
    {
        private readonly HotelContext _context;

        public PagosController(HotelContext context)
        {
            _context = context;
        }

        // GET: Pagos
        public async Task<IActionResult> Index()
        {
            var hotelContext = _context.Pagos.Include(p => p.Reserva);
            return View(await hotelContext.ToListAsync());
        }

        // GET: Pagos/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .Include(p => p.Reserva)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // GET: Pagos/Create
        public IActionResult Create()
        {
            ViewData["ReservaId"] = new SelectList(_context.Reservas, "Id", "Id");
            return View();
        }

        // POST: Pagos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ReservaId,MetodoPago,FechaPago,Monto,Estado")] Pago pago)
        {
            if (ModelState.IsValid)
            {
                pago.Id = Guid.NewGuid();
                _context.Add(pago);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReservaId"] = new SelectList(_context.Reservas, "Id", "Id", pago.ReservaId);
            return View(pago);
        }

        // GET: Pagos/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null)
            {
                return NotFound();
            }
            ViewData["ReservaId"] = new SelectList(_context.Reservas, "Id", "Id", pago.ReservaId);
            return View(pago);
        }

        // POST: Pagos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ReservaId,MetodoPago,FechaPago,Monto,Estado")] Pago pago)
        {
            if (id != pago.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pago);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagoExists(pago.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReservaId"] = new SelectList(_context.Reservas, "Id", "Id", pago.ReservaId);
            return View(pago);
        }

        // GET: Pagos/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .Include(p => p.Reserva)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // POST: Pagos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago != null)
            {
                _context.Pagos.Remove(pago);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PagoExists(Guid id)
        {
            return _context.Pagos.Any(e => e.Id == id);
        }
    }
}
