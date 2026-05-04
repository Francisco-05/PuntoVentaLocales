using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuntoVenta.Models
{
    public class SaleDetail
    {
        // Relación con el producto
        public int ProductId { get; set; }

        // Información del producto (snapshot al momento de la venta)
        public string Nombre { get; set; } = "";
        public string Marca { get; set; } = "";

        // Precio de venta por unidad
        public double PrecioUnitario { get; set; }

        // Costo por unidad (para calcular utilidad)
        public double CostoUnitario { get; set; }

        // Cantidad comprada
        public int Cantidad { get; set; }

        // Subtotal (precio * cantidad)
        public double Subtotal => PrecioUnitario * Cantidad;

        // Costo total (costo * cantidad)
        public double CostoTotal => CostoUnitario * Cantidad;

        // Ganancia por este producto
        public double Utilidad => Subtotal - CostoTotal;
    }
}