using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace appWeb2.Models
{
    public class VentasViewModel
    {
        [Key]
        public int id { get; set; }

        public DateTime fechaCompra { get; set; }
        public int UsuarioId { get; set; }

        public string NombreUsuario { get; set; }
        public int VideoJuegosId { get; set; }
        public string NombreVideoJuego { get; set; }

        public int cantidad { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal total { get; set; }

        public string estadoCompra { get; set; }

        public DateTime fechaHoraTransaccion { get; set; }

        public string codigoTransaccion { get; set; }
        public int idCompra { get; internal set; }
    }
}
