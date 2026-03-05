using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace appWeb2.Models
{
    public class Compra
    {
        [Key]
        public int id { get; set; }
        [Required]
        public DateTime fechaCompra { get; set; } = DateTime.Now;

        [Required]
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]

        public Usuario Usuario { get; set; }

        public int VideoJuegoId { get; set; }
        [ForeignKey("VideoJuegoId")]

        public VideoJuego VideoJuego { get; set; }


    }
}
