using PuntoVenta.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PuntoVenta.Services
{
    public static class UserService
    {
        private const string FILE = "users.json";

        public static async Task InitializeAsync()
        {
            var users = await JsonService.LoadAsync<User>(FILE);

            if (users.Count == 0)
            {
                users.Add(new User
                {
                    Id = 1,
                    Username = "admin123",
                    Password = "Admin123",
                    NombreCompleto = "Administrador",
                    Telefono = "0000000000",
                    FechaNacimiento = new System.DateTime(2000, 1, 1),
                    Rol = "Admin"
                });

                await JsonService.SaveAsync(FILE, users);
            }
        }

        public static async Task<User> Login(string username, string password)
        {
            var users = await JsonService.LoadAsync<User>(FILE);

            return users.FirstOrDefault(u =>
                u.Username == username && u.Password == password);
        }
    }
}