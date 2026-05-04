using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PuntoVenta.Services
{
    public static class JsonService
    {
        private static string GetDataPath()
        {
            // Busca el directorio del proyecto para colocar la carpeta "Data" allí
            DirectoryInfo? directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (directory != null)
            {
                // Verifica si el archivo "PuntoVenta.csproj" existe en el directorio actual
                string projectFile = Path.Combine(directory.FullName, "PuntoVenta.csproj");

                // Si se encuentra el archivo del proyecto, devuelve la ruta para la carpeta "Data" dentro de ese directorio
                if (File.Exists(projectFile))
                {
                    return Path.Combine(directory.FullName, "Data");
                }
                // Si no se encuentra, sube un nivel en el directorio
                directory = directory.Parent;
            }
            // Si no se encuentra el archivo del proyecto, devuelve una ruta predeterminada dentro del directorio base de la aplicación
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        }
        // Almacena la ruta base para los archivos JSON
        private static string basePath = GetDataPath();

        private static void EnsureFolder()
        {      
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
        }
        
        public static async Task<List<T>> LoadAsync<T>(string fileName)
        {
            
            EnsureFolder();
            
            string filePath = Path.Combine(basePath, fileName);

            if (!File.Exists(filePath))
            {   // Si el archivo no existe, lo crea con un contenido inicial de una lista vacía
                await File.WriteAllTextAsync(filePath, "[]");
                return new List<T>();
            }
                
            string json = await File.ReadAllTextAsync(filePath);

            // Si el contenido del archivo es nulo o solo contiene espacios en blanco, devuelve una lista vacía
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<T>();
            }
            
            return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
        }

        public static async Task SaveAsync<T>(string fileName, List<T> data)
        {
            EnsureFolder();
            // Combina la ruta base con el nombre del archivo para obtener la ruta completa del archivo JSON
            string filePath = Path.Combine(basePath, fileName);
            
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            await File.WriteAllTextAsync(filePath, json);
        }
    }
}