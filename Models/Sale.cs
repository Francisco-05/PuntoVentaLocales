using System;
using System.Collections.Generic;
using System.Linq;

namespace PuntoVenta.Models
{
    public class Sale
    {
        public int Id { get; set; }

        // Detalle de productos vendidos (carrito)
        public List<SaleDetail> Details { get; set; } = new List<SaleDetail>();

        // Empleado que realizó la venta
        public string Empleado { get; set; } = "";

        // Fecha de la venta
        public DateTime Fecha { get; set; } = DateTime.Now;

        // Método de pago (Efectivo / Tarjeta)
        public string MetodoPago { get; set; } = "";

        // Cantidad total de artículos
        public int TotalArticulos => Details.Sum(d => d.Cantidad);

        // Total bruto (precio de venta)
        public double TotalBruto => Details.Sum(d => d.PrecioUnitario * d.Cantidad);

        // Total de costos
        public double TotalCosto => Details.Sum(d => d.CostoUnitario * d.Cantidad);

        // Utilidad neta
        public double Utilidad => Details.Sum(d => (d.PrecioUnitario - d.CostoUnitario) * d.Cantidad);
    }
}