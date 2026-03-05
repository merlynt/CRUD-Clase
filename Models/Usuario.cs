using System.ComponentModel.DataAnnotations;

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
        public string contrasena { get; set; }
        [Required]

        public DateTime fechaRegistro { get; set; } = DateTime.Now;

        public ICollection<Compra> compras { get; set; }
    }
}
