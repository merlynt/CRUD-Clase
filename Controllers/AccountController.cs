using System.Security.Cryptography;
using System.Text;
using appWeb2.Data;
using appWeb2.Filters;
using appWeb2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace appWeb2.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        private void ObtenerCategorias()
        {
            var data = _context.Categorias.ToList();
            ViewBag.CategoriasSelect = new SelectList(data, "id", "categoria");
        }
        [HttpGet]

        public IActionResult Login()
        {
            ObtenerCategorias();
            return View();
        }

        [SessionAuthorize]
        public IActionResult Dashboard()
        {
            //var data = (from v in _context.VideoJuegos 
            //            join c in _context.Categorias
            //            on v.idCategoria equals c.id
            //            group v by c.categoria into g
            //            select new
            //            {
            //                Categoria = g.Key,
            //                Total = g.Count()
            //            }).ToList();
            //ViewBag.Categorias = data.Select(x => x.Categoria).ToList();
            //ViewBag.Totales = data.Select(x => x.Total).ToList();
            ObtenerCategorias();
            return View();
        }


        public IActionResult ObtenerDatos(int? id) 
        {
            var query = from v in _context.VideoJuegos
                        join c in _context.Categorias
                        on v.idCategoria equals c.id
                        select new { v.idCategoria, c.categoria };

         
            if (id.HasValue && id > 0)
            {
                query = query.Where(x => x.idCategoria == id);
            }

            var data = query.GroupBy(x => x.categoria)
                            .Select(g => new
                            {
                                categoria = g.Key,
                                total = g.Count()
                            }).ToList();

            return Json(data);
        }
        //public IActionResult ObtenerVentasPastel()
        //{
        //    var data = _context.Compras
        //        .GroupBy(c => c.VideoJuego.titulo)
        //        .Select(g => new
        //        {
        //            name = g.Key,      
        //            y = g.Count()      
        //        })
        //        .OrderByDescending(x => x.y)
        //        .Take(5)
        //        .ToList();

        //    return Json(data);
        //}
        public IActionResult ObtenerRegistroUsuarios(string periodo)
        {
            var query = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrEmpty(periodo))
            {
                var partes = periodo.Split('-');
                int anio = int.Parse(partes[0]);
                int mes = int.Parse(partes[1]);

                var datosPorDia = query
                    .Where(u => u.fechaRegistro.Value.Year == anio && u.fechaRegistro.Value.Month == mes)
                    //                         ^^^^^^                         ^^^^^^
                    .GroupBy(u => u.fechaRegistro.Value.Day)
                    //                           ^^^^^^
                    .Select(g => new {
                        etiqueta = "Día " + g.Key,
                        total = g.Count()
                    })
                    .OrderBy(x => x.etiqueta).ToList();

                return Json(datosPorDia);
            }
            var datosAnuales = query
                .Where(u => u.fechaRegistro.Value.Year == 2026)
                .GroupBy(u => u.fechaRegistro.Value.Month)
                .Select(g => new {
                    etiqueta = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                    total = g.Count(),
                    mesN = g.Key
                })
                .OrderBy(x => x.mesN).ToList();

            return Json(datosAnuales);
        }

        //public IActionResult ObtenerIngresosPorCategoria()
        //{
        //    var data = _context.Compras
        //        .GroupBy(c => c.VideoJuegos.Categoria.categoria) 
        //        .Select(g => new
        //        {
        //            name = g.Key,
        //            y = g.Sum(x => x.VideoJuego.precio)
        //        })
        //        .OrderByDescending(x => x.y)
        //        .ToList();

        //    return Json(data);
        //}
        public async Task<IActionResult> DetalleVentas(DateTime? desde, DateTime? hasta, int pagina = 1)
        {
            int paginador = 10;

            var query = _context.DetallesCompra
                .Include(d => d.Compra)
                .Include(c => c.VideoJuego)
                .AsQueryable();

            if (desde.HasValue)
            {
                query = query.Where(d => d.fechaHoraTransaccion >= desde.Value);
            }

            if (hasta.HasValue)
            {
                query = query.Where(d => d.fechaHoraTransaccion <= hasta.Value);
            }

            var totalregistros = await query.CountAsync();

            var datos = await query
                .OrderByDescending(d => d.fechaHoraTransaccion)
                .Skip((pagina - 1) * paginador)
                .Take(paginador)
                .Select(d => new VentasViewModel
                {
                  idCompra = d.idCompra,
                  VideoJuegosId = d.VideoJuegosId,
                  cantidad = d.cantidad,
                  total = d.total,
                  estadoCompra = d.estadoCompra,
                  fechaHoraTransaccion = d.fechaHoraTransaccion,
                  codigoTransaccion = d.codigoTransaccion
                }).ToListAsync();

            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalregistros / paginador);
            ViewBag.PaginaActual = pagina;
            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;

            return View(datos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Login(Login model)
        {
           
            var user = _context.Usuarios
            .FirstOrDefault(u => u.correo == model.correo);

            if(user != null)
            {
                string saltedContrasena = user.salt + model.contrasena;
                using (SHA256 sha256 = SHA256.Create())
                { 
                    //byte[] inputBytes = Encoding.UTF8.GetBytes(saltedContrasena);
                    byte[] inputBytes = Encoding.Unicode.GetBytes(saltedContrasena);
                    byte[] hashBytes = sha256.ComputeHash(inputBytes);
                    Console.WriteLine("Salt DB: " + user.salt);
                    Console.WriteLine("Password input: " + model.contrasena);
                    Console.WriteLine("Salted: " + (user.salt + model.contrasena));

                    Console.WriteLine("Hash generado: " + Convert.ToBase64String(hashBytes));
                    Console.WriteLine("Hash DB: " + Convert.ToBase64String(user.contrasena));

                    if (hashBytes.SequenceEqual(user.contrasena))
                    {
                        HttpContext.Session.SetString("usuario", user.nombre);
    
                        return RedirectToAction("Index", "Home");
                    }
                    

                }
            }
            
            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View("Login");
           

        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Login", "Account");
        }
    }
}
