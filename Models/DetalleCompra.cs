using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace appWeb2.Models
{
    [Table("detalle_compra")]
    public class DetalleCompra
    {

        [Key]
        public int id { get; set; }

        [Required]
        public int VideoJuegosId { get; set; }

        [Required]
        public int cantidad { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal total { get; set; }

        [StringLength(50)]
        public string estadoCompra { get; set; }

        public DateTime fechaHoraTransaccion { get; set; }

        [StringLength(100)]
        public string codigoTransaccion { get; set; }

        [Required]
        
        public int idCompra { get; set; }

        [ForeignKey("VideoJuegosId")]
        public virtual VideoJuego VideoJuego { get; set; }

        [ForeignKey("idCompra")]
        public virtual Compra Compra { get; set; }
    }
}
