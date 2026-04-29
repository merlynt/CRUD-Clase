using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace appWeb2.Models
{
    public class Usuario
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(200)]
        public string nombre { get; set; }
        [Required]
        public string correo { get; set; }
        [Required]
        public byte[] contrasena { get; set; }
        [Required]

        [ForeignKey("idRol")]
        public Rol rol { get; set; }
        public int idRol { get; set; }
        
        public string salt { get; set; }

        public DateTime? fechaRegistro { get; set; } = DateTime.Now;

        public ICollection<Compra> compras { get; set; }
    }
}
