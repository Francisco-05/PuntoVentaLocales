using System;

namespace PuntoVenta.Models
{
    public class DiferenciaCaja
    {
        public int Id { get; set; }

        public string Empleado { get; set; }

        public DateTime Fecha { get; set; }

        public DateTime InicioSesion { get; set; }

        public DateTime FinSesion { get; set; }

        public double EfectivoSistema { get; set; }

        public double EfectivoReal { get; set; }

        public double Diferencia { get; set; }
    }
}