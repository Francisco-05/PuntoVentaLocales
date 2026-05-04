using PuntoVenta.Models;
using PuntoVenta.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PuntoVenta.Services
{
    public static class UserService
    {
        private const string FILE = "users.json";
        //  Inicializa el archivo de usuarios con un admin por defecto si está vacío
        public static async Task InitializeAsync()
        {
            var users = await JsonService.LoadAsync<User>(FILE);

            if (users.Count == 0)
            {
                users.Add(new User
                {
                    //  El ID se genera automáticamente para asegurar unicidad
                    Id = IdGenerator.GetNextId(users),
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
            //  Busca un usuario que coincida con el username y password proporcionados
            return users.FirstOrDefault(u =>
                u.Username == username && u.Password == password);
        }

        //  Agrega un nuevo usuario al sistema
        public static async Task CreateUserAsync(User newUser)
        {
            var users = await JsonService.LoadAsync<User>(FILE);

            newUser.Id = IdGenerator.GetNextId(users);

            users.Add(newUser);

            await JsonService.SaveAsync(FILE, users);
        }
    }
}