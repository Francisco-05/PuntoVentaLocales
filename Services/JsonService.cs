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
            DirectoryInfo? directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (directory != null)
            {
                string projectFile = Path.Combine(directory.FullName, "PuntoVenta.csproj");

                if (File.Exists(projectFile))
                {
                    return Path.Combine(directory.FullName, "Data");
                }

                directory = directory.Parent;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        }

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

        public static async Task SaveAsync<T>(string fileName, List<T> data)
        {
            EnsureFolder();

            string filePath = Path.Combine(basePath, fileName);

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            await File.WriteAllTextAsync(filePath, json);
        }
    }
}