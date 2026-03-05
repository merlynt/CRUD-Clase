using System.ComponentModel.DataAnnotations;

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
        //public ICollection<Compra> Compras { get; set; }


    }
}
