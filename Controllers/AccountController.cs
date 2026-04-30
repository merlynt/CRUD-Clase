using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
        public IActionResult JuegosVentas()
        {

            return View();
        }
        public IActionResult Register()
        {

            return View();
        }

        public IActionResult Login()
        {
            ObtenerCategorias();
            return View();
        }

        [SessionAuthorize(1)]
        public IActionResult Dashboard()
        {
             ObtenerCategorias();
            string jsonUsuario = HttpContext.Session.GetString("usuario");
            if (string.IsNullOrEmpty(jsonUsuario))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = JsonSerializer.Deserialize<Usuario>(jsonUsuario);

            if (user.rol.id != 1)
            {
                return RedirectToAction("JuegosVentas");
            }
            else
            {
                return View();
            }
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
        public IActionResult ObtenerVentasPastel()
        {
            var data = _context.DetallesCompra
                .GroupBy(c => c.VideoJuego.titulo)
                .Select(g => new
                {
                    name = g.Key,
                    y = g.Sum(c => c.cantidad)
                })
                .OrderByDescending(x => x.y)
                .Take(5)
                .ToList();

            return Json(data);
        }
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

        public IActionResult ObtenerIngresosPorCategoria()
        {
            var data = _context.DetallesCompra
                .GroupBy(c => c.VideoJuego.Categoria.categoria) 
                .Select(g => new
                {
                    name = g.Key,
                    y = g.Sum(x => x.VideoJuego.precio * x.cantidad)
                })
                .OrderByDescending(x => x.y)
                .ToList();

            return Json(data);
        }

        [SessionAuthorize(1)]
        public async Task<IActionResult> DetalleVentas(DateTime? desde, DateTime? hasta, string? cliente, string? videojuego, int pagina = 1)
        {
            int paginador = 10;

            var query = _context.DetallesCompra
                .Include(d => d.Compra)
                    .ThenInclude(c => c.Usuario)
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

            if (!string.IsNullOrWhiteSpace(cliente))
            {
                query = query.Where(d => d.Compra.Usuario.nombre.Contains(cliente));
            }

            if (!string.IsNullOrWhiteSpace(videojuego))
            {
                query = query.Where(d => d.VideoJuego.titulo.Contains(videojuego));
            }

            var totalregistros = await query.CountAsync();

            var datos = await query
                .OrderByDescending(d => d.fechaHoraTransaccion)
                .Skip((pagina - 1) * paginador)
                .Take(paginador)
                .Select(d => new VentasViewModel
                {
                    idCompra = d.idCompra,
                    NombreUsuario = d.Compra.Usuario.nombre,
                    NombreVideoJuego = d.VideoJuego.titulo,
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
            ViewBag.Cliente = cliente;
            ViewBag.Videojuego = videojuego;

            return View(datos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Login(Login model)
        {
           
            var user = _context.Usuarios
            .Include(u => u.rol)
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

                        //HttpContext.Session.SetString("usuario", user.nombre);
                        string jsonUsuario = JsonSerializer.Serialize(user);
                        HttpContext.Session.SetString("usuario", jsonUsuario);

                        if(user.rol.id == 1)
                        {
                            return RedirectToAction("Dashboard", "Account");
                        }else if(user.rol.id == 2)
                        {

                            return RedirectToAction("Index", "Home");

                        }
                    }
                    

                }
            }
            
            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View("Login");
           

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuarioExistente = _context.Usuarios
                .FirstOrDefault(u => u.correo == model.correo);

            if (usuarioExistente != null)
            {
                ViewBag.Error = "Este correo electrónico ya está registrado.";
                return View(model);
            }

            string nuevoSalt = Guid.NewGuid().ToString("N").Substring(0, 10);

            string saltedContrasena = nuevoSalt + model.contrasena;

            byte[] hashBytes;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.Unicode.GetBytes(saltedContrasena);
                hashBytes = sha256.ComputeHash(inputBytes);
            }

            var nuevoUsuario = new Usuario
            {
                nombre = model.nombre,
                correo = model.correo,
                contrasena = hashBytes, 
                salt = nuevoSalt,       
                idRol = 2,            
                fechaRegistro = DateTime.Now
            };

            try
            {
                _context.Usuarios.Add(nuevoUsuario);
                _context.SaveChanges();

                TempData["MensajeExito"] = "Cuenta creada exitosamente. Por favor, inicia sesión.";

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al registrar: " + ex.Message);
                ViewBag.Error = "Ocurrió un error al crear la cuenta. Inténtalo de nuevo.";
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> MisCompras(string buscar, DateTime? desde, DateTime? hasta)
        {
            var usuarioJson = HttpContext.Session.GetString("usuario");
            if (string.IsNullOrEmpty(usuarioJson)) return RedirectToAction("Login", "Account");

            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var usuarioLogueado = JsonSerializer.Deserialize<Usuario>(usuarioJson, opciones);

            var query = _context.DetallesCompra
                .Include(d => d.VideoJuego) 
                .Include(d => d.Compra)    
                .Where(d => d.Compra.UsuarioId == usuarioLogueado.id);

            // 3. Filtros
            if (!string.IsNullOrEmpty(buscar))
                query = query.Where(d => d.VideoJuego.titulo.Contains(buscar));

            if (desde.HasValue)
                query = query.Where(d => d.fechaHoraTransaccion.Date >= desde.Value.Date);

            if (hasta.HasValue)
                query = query.Where(d => d.fechaHoraTransaccion.Date <= hasta.Value.Date);

            var misDetalles = await query.OrderByDescending(d => d.fechaHoraTransaccion).ToListAsync();

            ViewBag.TotalJuegos = misDetalles.Sum(d => d.cantidad);
            ViewBag.TotalGastado = misDetalles.Sum(d => d.total);

            ViewBag.GastadoMes = misDetalles
                .Where(d => d.fechaHoraTransaccion.Month == DateTime.Now.Month && d.fechaHoraTransaccion.Year == DateTime.Now.Year)
                .Sum(d => d.total);

            return View(misDetalles);
        }

        [HttpGet]
        public IActionResult Gestionar()
        {
            var usuarioJson = HttpContext.Session.GetString("usuario");
            if (string.IsNullOrEmpty(usuarioJson)) return RedirectToAction("Login", "Account");

            var usuario = JsonSerializer.Deserialize<Usuario>(usuarioJson);

            var model = new GestionCuentaViewModel
            {
                Nombre = usuario.nombre
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Gestionar(GestionCuentaViewModel model)
        {
            var usuarioJson = HttpContext.Session.GetString("usuario");
            if (string.IsNullOrEmpty(usuarioJson)) return RedirectToAction("Login", "Account");

            var usuarioSesion = JsonSerializer.Deserialize<Usuario>(usuarioJson);

            var usuarioDb = _context.Usuarios.FirstOrDefault(u => u.id == usuarioSesion.id);
            if (usuarioDb == null) return RedirectToAction("Login", "Account");

            usuarioDb.nombre = model.Nombre;

            if (!string.IsNullOrEmpty(model.NuevaPassword))
            {

                if (string.IsNullOrEmpty(model.PasswordActual))
                {
                    ViewBag.Error = "Debes ingresar tu contraseña actual para poder cambiarla.";
                    return View(model);
                }

                string saltedActual = usuarioDb.salt + model.PasswordActual;
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] inputBytesActual = Encoding.Unicode.GetBytes(saltedActual);
                    byte[] hashActual = sha256.ComputeHash(inputBytesActual);

                    if (!hashActual.SequenceEqual(usuarioDb.contrasena))
                    {
                        ViewBag.Error = "La contraseña actual es incorrecta.";
                        return View(model);
                    }
                }

                if (model.NuevaPassword != model.ConfirmarPassword)
                {
                    ViewBag.Error = "Las nuevas contraseñas no coinciden.";
                    return View(model);
                }

                string nuevoSalt = Guid.NewGuid().ToString("N").Substring(0, 10);
                string saltedNueva = nuevoSalt + model.NuevaPassword;

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] inputBytesNueva = Encoding.Unicode.GetBytes(saltedNueva);
                    byte[] hashNueva = sha256.ComputeHash(inputBytesNueva);

                    usuarioDb.salt = nuevoSalt;
                    usuarioDb.contrasena = hashNueva;
                }
            }

            _context.Usuarios.Update(usuarioDb);
            _context.SaveChanges();

            string nuevoJson = JsonSerializer.Serialize(usuarioDb);
            HttpContext.Session.SetString("usuario", nuevoJson);

            ViewBag.MensajeExito = "Tu cuenta se ha actualizado correctamente.";
            return View(model);
        }
    }
}
