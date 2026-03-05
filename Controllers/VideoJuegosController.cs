using appWeb2.Data;
using appWeb2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace appWeb2.Controllers
{
    public class VideoJuegosController : Controller
    {
        private readonly AppDbContext _context;
        public VideoJuegosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var juegos = await _context.VideoJuegos.ToListAsync();
            return View(juegos);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]

        public async Task<IActionResult> Create(VideoJuego juego)
        {
            if (ModelState.IsValid)
            {
                _context.Add(juego);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(juego);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var juego = await _context.VideoJuegos.FindAsync(id);
            if (juego == null) return NotFound();

            return View(juego);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, VideoJuego juego)
        {
            if (id != juego.id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(juego);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.VideoJuegos.Any(e => e.id == juego.id))

                        return NotFound();
                    else
                        throw;

                }
                return RedirectToAction(nameof(Index));
            }
            return View(juego);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var juego = await _context.VideoJuegos.FindAsync(id);
            if (juego == null) return NotFound();
            return View(juego);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var juego = await _context.VideoJuegos.FindAsync(id);
            if (juego != null)
            {
                _context.VideoJuegos.Remove(juego);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));

        }
    }
}
