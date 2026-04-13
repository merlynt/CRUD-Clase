using System.Security.Cryptography;
using System.Text;
using appWeb2.Data;
using appWeb2.Filters;
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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [SessionAuthorize]
        public IActionResult Dashboard()
        {
            return View();
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
