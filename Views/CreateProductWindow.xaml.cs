using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;

namespace PuntoVenta.Views
{
    public sealed partial class CreateProductWindow : Window
    {
        private string selectedImagePath = "";

        public CreateProductWindow()
        {
            this.InitializeComponent();
        }

        // 🖼 SELECT IMAGE (CORREGIDO)
        private async void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();

            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                string assetsFolder = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Assets",
                    "Products"
                );

                if (!Directory.Exists(assetsFolder))
                    Directory.CreateDirectory(assetsFolder);

                string newFileName = Guid.NewGuid() + Path.GetExtension(file.Name);
                string destinationPath = Path.Combine(assetsFolder, newFileName);

                // 🔥 FIX REAL (EVITA CRASH WIN32)
                using (var stream = await file.OpenStreamForReadAsync())
                using (var fileStream = File.Create(destinationPath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                selectedImagePath = destinationPath;
                ImageBox.Text = newFileName;
            }
        }

        // 💾 SAVE PRODUCT
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var products = await JsonService.LoadAsync<Product>("products.json");

            // 🔥 CAMPOS VACÍOS
            if (string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(BrandBox.Text) ||
                string.IsNullOrWhiteSpace(CostBox.Text) ||
                string.IsNullOrWhiteSpace(PriceBox.Text))
            {
                await ShowError("Todos los campos son obligatorios");
                return;
            }

            // 🔥 NUMÉRICOS
            if (!double.TryParse(CostBox.Text, out double costo))
            {
                await ShowError("Costo inválido");
                return;
            }

            if (!double.TryParse(PriceBox.Text, out double precio))
            {
                await ShowError("Precio inválido");
                return;
            }

            if (costo < 0 || precio < 0)
            {
                await ShowError("No se permiten valores negativos");
                return;
            }

            // 🔥 DUPLICADO (NOMBRE + MARCA)
            bool exists = products.Exists(p =>
                p.Nombre.ToLower() == NameBox.Text.ToLower() &&
                p.Marca.ToLower() == BrandBox.Text.ToLower()
            );

            if (exists)
            {
                await ShowError("Ya existe un producto con ese nombre y marca");
                return;
            }

            var product = new Product
            {
                Id = products.Count + 1,
                Nombre = NameBox.Text,
                Marca = BrandBox.Text,
                Descripcion = DescBox.Text,
                Costo = costo,
                PrecioVenta = precio,
                Imagen = selectedImagePath
            };

            products.Add(product);

            await JsonService.SaveAsync("products.json", products);

            this.Close();
        }

        // ⚠️ ERROR DIALOG
        private async Task ShowError(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}