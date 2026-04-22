using appWeb2.Data;
using appWeb2.Filters;
using appWeb2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace appWeb2.Controllers
{
    [SessionAuthorize]
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

        public async Task<IActionResult> Create(VideoJuego juego, IFormFile archivoImagen)
        {
            if (!ModelState.IsValid) return View(juego);

            if (archivoImagen != null && archivoImagen.Length > 0)
            {
                var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);
                var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", nombreArchivo);
                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    await archivoImagen.CopyToAsync(stream);
                }
                juego.imagen = "/images/" + nombreArchivo;
            }

            _context.VideoJuegos.Add(juego);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        public async Task<IActionResult> Edit(int id, VideoJuego juego, IFormFile? archivoImagen)
        {
            if (id != juego.id)
                return NotFound();

            var juegoDB = await _context.VideoJuegos.FindAsync(id);

            if (juegoDB == null)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                juegoDB.titulo = juego.titulo;
                juegoDB.precio = juego.precio;
                juegoDB.categoria = juego.categoria;
                juegoDB.idCategoria = juego.idCategoria;
                juegoDB.descripcion = juego.descripcion;
                juegoDB.fechaLanzamiento = juego.fechaLanzamiento;
                juegoDB.porcentajeDescuento = juego.porcentajeDescuento;
                juegoDB.clasificacionEdad = juego.clasificacionEdad;

                if (archivoImagen != null && archivoImagen.Length > 0)
                {
                    if (!string.IsNullOrEmpty(juegoDB.imagen))
                    {
                        var rutaAnterior = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            juegoDB.imagen.TrimStart('/')
                        );


                    }

                    var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);
                    var ruta = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/images",
                        nombreArchivo
                    );

                    using (var stream = new FileStream(ruta, FileMode.Create))
                    {
                        await archivoImagen.CopyToAsync(stream);
                    }
                    juegoDB.imagen = "/images/" + nombreArchivo;
                }
                await _context.SaveChangesAsync();
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
