using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
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

        // Seleccionar imagen
        private async void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();

            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hwnd);
            // Filtros para tipos de imagen
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                // Carpeta segura para almacenar imágenes
                string assetsFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "PuntoVenta",
                    "Products"
                );

                if (!Directory.Exists(assetsFolder))
                    Directory.CreateDirectory(assetsFolder);

                // Generar nombre único para evitar colisiones
                string newFileName = Guid.NewGuid() + Path.GetExtension(file.Name);
                string destinationPath = Path.Combine(assetsFolder, newFileName);

                //Copiar imagen de forma segura
                using (var stream = await file.OpenStreamForReadAsync())
                using (var fileStream = File.Create(destinationPath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                // Guarda datos
                selectedImagePath = destinationPath; // ruta completa
                ImageBox.Text = newFileName;         // solo nombre 
            }
        }
        // Validación de solo números y punto decimal
        private void OnlyNumbersDecimal_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            string text = args.NewText;

            // Permitir solo números y punto
            if (text.Any(c => !char.IsDigit(c) && c != '.'))
            {
                args.Cancel = true;
                return;
            }

            // Permitir solo un punto decimal
            if (text.Count(c => c == '.') > 1)
            {
                args.Cancel = true;
            }
        }
        // Guardar producto con validaciones
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var products = await JsonService.LoadAsync<Product>("products.json");

            // CAMPOS VACÍOS
            if (string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(BrandBox.Text) ||
                string.IsNullOrWhiteSpace(CostBox.Text) ||
                string.IsNullOrWhiteSpace(PriceBox.Text))
            {
                await ShowError("Todos los campos son obligatorios");
                return;
            }

            // NUMÉRICOS
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

            if (precio <= costo)
            {
                await ShowError("El precio de venta no puede ser igual o menor al costo");
                return;
            }
            if (costo < 1)
            {
                await ShowError("El costo no puede ser menor a $1.00");
                return;
            }

            // Verificar si ya existe un producto con el mismo nombre y marca (ignorar mayúsculas)
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

        // Mostrar diálogo de error
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