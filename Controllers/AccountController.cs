using System.Security.Cryptography;
using System.Text;
using appWeb2.Data;
using appWeb2.Models;
using Microsoft.AspNetCore.Mvc;

namespace appWeb2.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]

        public IActionResult Login(Login model)
        {
            /*
            var user = _context.Usuarios.FirstOrDefault(u => u.correo == correo && u.contrasena == contrasena);
            if (user != null)
            {
                HttpContext.Session.SetString("usuario", user.nombre);
                Console.WriteLine($"Usuario {user.nombre} ha iniciado sesión.");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View("Index");
            }
            */

            var user = _context.Usuarios
            .FirstOrDefault(u => u.correo == model.correo);

            if(user != null)
            {
                string saltedContrasena = user.salt + model.contrasena;
                using (SHA256 sha256 = SHA256.Create())
                { 
                    byte[] inputBytes = Encoding.UTF8.GetBytes(saltedContrasena);
                    byte[] hashBytes = sha256.ComputeHash(inputBytes);

                    if(hashBytes.SequenceEqual(user.contrasena))
                    {
                        HttpContext.Session.SetString("usuario", user.nombre);
    
                        return RedirectToAction("Index", "Home");
                    }
                    

                }
            }
            
            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View("Index");
           

        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
