using System;
using PuntoVenta.Models;

namespace PuntoVenta.Services
{
    public static class SessionService
    {
        public static User CurrentUser { get; set; }

        // Hora en que inicia sesión
        public static DateTime LoginTime { get; set; }
    }
}