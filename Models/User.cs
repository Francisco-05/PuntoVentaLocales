using Microsoft.UI.Xaml;
using System;

namespace PuntoVenta.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string NombreCompleto { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Rol { get; set; } = "Empleado";



        public bool IsNotAdmin =>
            Rol != "Admin";

        // Para mostrar u ocultar elementos en la UI según el rol
        public Visibility IsAdminVisibility =>
            Rol == "Admin"
                ? Visibility.Visible
                : Visibility.Collapsed;
    }
}