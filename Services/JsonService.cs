using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PuntoVenta.Services
{
    public static class JsonService
    {
        //  Busca el directorio del proyecto para guardar los archivos JSON
        private static string GetDataPath()
        {
            DirectoryInfo? directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            //  Subir directorios hasta encontrar el proyecto
            while (directory != null)
            {
                string projectFile = Path.Combine(directory.FullName, "PuntoVenta.csproj");

                if (File.Exists(projectFile))
                {
                    return Path.Combine(directory.FullName, "Data");
                }

                directory = directory.Parent;
            }
            //  Si no se encuentra el proyecto, usar una carpeta en Documentos
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        }
        //  Ruta base para los archivos JSON
        private static string basePath = GetDataPath();

        //  Asegura que la carpeta exista antes de leer o escribir archivos
        private static void EnsureFolder()
        {
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
        }
        //  Carga una lista de objetos desde un archivo JSON, o crea el archivo si no existe
        public static async Task<List<T>> LoadAsync<T>(string fileName)
        {
            EnsureFolder();

            string filePath = Path.Combine(basePath, fileName);

            if (!File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath, "[]");
                return new List<T>();
            }

            string json = await File.ReadAllTextAsync(filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<T>();
            }

            return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
        }
        //  Guarda una lista de objetos en un archivo JSON
        public static async Task SaveAsync<T>(string fileName, List<T> data)
        {
            EnsureFolder();

            string filePath = Path.Combine(basePath, fileName);

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            await File.WriteAllTextAsync(filePath, json);
        }
    }
}