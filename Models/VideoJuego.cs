using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace appWeb2.Models
{
    public class VideoJuego
    {
        [Key]
        public int id { get; set; }
        [Required]
        [StringLength(200)]
        public string titulo { get; set; }
        [Required] 
    
        public decimal precio { get; set; }
        [Required]

        public string categoria { get; set; }
        [Required]
        public string descripcion { get; set; }

        [Column("id_categoria")]
        public int idCategoria { get; set; }

        [ForeignKey("idCategoria")]
        public Categoria Categoria { get; set; }

        [Required]
        [Column("fecha_lanzamiento")]
        public DateOnly fechaLanzamiento { get; set; }

        [Column("porcentaje_descuento")]
        public decimal? porcentajeDescuento { get; set; }

        [Column("clasificacion_edad")]
        public string? clasificacionEdad { get; set; }
        public string? imagen { get; set; }
        //public ICollection<Compra> Compras { get; set; }


    }
}
