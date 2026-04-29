using appWeb2.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace appWeb2.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;
        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Agregamos Include para que traiga la información de la tabla Rol
            var usuarios = await _context.Usuarios.Include(u => u.rol).ToListAsync();
            return View(usuarios);
        }
    }
}