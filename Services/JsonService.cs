using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PuntoVenta.Services
{
    public static class JsonService
    {
        // 🔥 RUTA FIJA (TU CARPETA)
        private static string basePath =
    Path.Combine(AppContext.BaseDirectory, "Data");

        // 📁 ASEGURAR CARPETA
        private static void EnsureFolder()
        {
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
        }

        // 📖 LEER JSON
        public static async Task<List<T>> LoadAsync<T>(string fileName)
        {
            EnsureFolder();

            string path = Path.Combine(basePath, fileName);

            if (!File.Exists(path))
                return new List<T>();

            string json = await File.ReadAllTextAsync(path);

            return JsonConvert.DeserializeObject<List<T>>(json)
                   ?? new List<T>();
        }

        // 💾 GUARDAR JSON
        public static async Task SaveAsync<T>(string fileName, List<T> data)
        {
            EnsureFolder();

            string path = Path.Combine(basePath, fileName);

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            await File.WriteAllTextAsync(path, json);
        }
    }
}