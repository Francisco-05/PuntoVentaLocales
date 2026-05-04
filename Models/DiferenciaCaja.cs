using System;

namespace PuntoVenta.Models
{
    public class DiferenciaCaja
    {
        public int Id { get; set; }

        // Empleado que hizo el corte
        public string Empleado { get; set; }

        // Fecha del corte
        public DateTime Fecha { get; set; }

        // Inicio y fin de sesión
        public DateTime InicioSesion { get; set; }
        public DateTime FinSesion { get; set; }

        // Datos de efectivo
        public double EfectivoSistema { get; set; } // lo que debería haber
        public double EfectivoReal { get; set; }    // lo que el usuario reporta

        // Diferencia
        public double Diferencia { get; set; }
    }
}