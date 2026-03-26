using System.Diagnostics;
using appWeb2.Data;
using appWeb2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace appWeb2.Controllers
{
    public class HomeController : Controller
    {
        // private readonly ILogger<HomeController> _logger;

        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.CategoriasDisponibles = await _context.VideoJuegos
                                          .Select(j => j.categoria)
                                          .Distinct()
                                          .OrderBy(c => c)
                                          .ToListAsync();

            var juegos = await _context.VideoJuegos.ToListAsync();
            return View(juegos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contacts()
        {
            return View();
        }

        public IActionResult VideoJuegos()
        {
        
            return RedirectToAction("Index", "VideoJuegos");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public async Task<IActionResult> categoryFilter(string category)
        {
            ViewBag.CategoriasDisponibles = await _context.VideoJuegos
                                          .Select(j => j.categoria)
                                          .Distinct()
                                          .OrderBy(c => c)
                                          .ToListAsync();

            if (string.IsNullOrEmpty(category))
            {
                return RedirectToAction("Index");
            }

            var juegos = await _context.VideoJuegos
                                       .Where(j => j.categoria == category)
                                       .ToListAsync();

            return View("Index",juegos);
        }
        public async Task<IActionResult> Novedades()
        {
            var juegosNuevos = await _context.VideoJuegos
                .OrderByDescending(j => j.fechaLanzamiento)
                .Take(15)
                .ToListAsync();

            return View("Index", juegosNuevos);
        }

        public async Task<IActionResult> promociones(decimal minDescuento)
        {
            var juegos = await _context.VideoJuegos
                                       .Where(j => j.porcentajeDescuento.HasValue &&
                                                   j.porcentajeDescuento == minDescuento)
                                       .ToListAsync();

            return View("Index", juegos);
        }

    }
}
