using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuntoVenta.Models
{
    public class RestockLog
    {
        public Guid Id { get; set; }

        public string Producto { get; set; }

        public int ExistenciasIniciales { get; set; }

        public int ExistenciasAgregadas { get; set; }

        public int ExistenciasFinales { get; set; }

        public DateTime FechaModificacion { get; set; }

        public string TipoMovimiento { get; set; }
    }
}