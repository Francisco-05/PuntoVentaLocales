using PuntoVenta.Models;
using PuntoVenta.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PuntoVenta.Services
{
    public static class UserService
    {
       //Almacena los usuarios
        private const string FILE = "users.json";

        public static async Task InitializeAsync()
        {   // Carga los usuarios desde el archivo JSON
            var users = await JsonService.LoadAsync<User>(FILE);

            if (users.Count == 0)
            {
                // Si no hay usuarios, crea un usuario admin por defecto
                users.Add(new User
                {
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

            // Busca un usuario que coincida con el nombre de usuario y la contraseña proporcionados
            return users.FirstOrDefault(u =>
                u.Username == username && u.Password == password);
        }

        public static async Task CreateUserAsync(User newUser)
        {   
            var users = await JsonService.LoadAsync<User>(FILE);
            // Asigna un nuevo ID al usuario utilizando el IdGenerator
            newUser.Id = IdGenerator.GetNextId(users);

            users.Add(newUser);

            await JsonService.SaveAsync(FILE, users);
        }
    }
}