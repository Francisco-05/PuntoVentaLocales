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

            // 🔥 BLOQUEAR ESPACIO COMO PRIMER CARACTER
            NameBox.TextChanging += (s, e) =>
            {
                if (NameBox.Text.StartsWith(" "))
                {
                    NameBox.Text = NameBox.Text.TrimStart();
                    NameBox.SelectionStart = NameBox.Text.Length;
                }
            };

            BrandBox.TextChanging += (s, e) =>
            {
                if (BrandBox.Text.StartsWith(" "))
                {
                    BrandBox.Text = BrandBox.Text.TrimStart();
                    BrandBox.SelectionStart = BrandBox.Text.Length;
                }
            };

            DescBox.TextChanging += (s, e) =>
            {
                if (DescBox.Text.StartsWith(" "))
                {
                    DescBox.Text = DescBox.Text.TrimStart();
                    DescBox.SelectionStart = DescBox.Text.Length;
                }
            };

            CostBox.TextChanging += (s, e) =>
            {
                if (CostBox.Text.StartsWith(" "))
                {
                    CostBox.Text = CostBox.Text.TrimStart();
                    CostBox.SelectionStart = CostBox.Text.Length;
                }
            };

            PriceBox.TextChanging += (s, e) =>
            {
                if (PriceBox.Text.StartsWith(" "))
                {
                    PriceBox.Text = PriceBox.Text.TrimStart();
                    PriceBox.SelectionStart = PriceBox.Text.Length;
                }
            };

            ImageBox.TextChanging += (s, e) =>
            {
                if (ImageBox.Text.StartsWith(" "))
                {
                    ImageBox.Text = ImageBox.Text.TrimStart();
                    ImageBox.SelectionStart = ImageBox.Text.Length;
                }
            };

            SetupEnterNavigation();
        }

        private void SetupEnterNavigation()
        {
            NameBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    BrandBox.Focus(FocusState.Programmatic);
                }
            };

            BrandBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    DescBox.Focus(FocusState.Programmatic);
                }
            };

            DescBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    CostBox.Focus(FocusState.Programmatic);
                }
            };

            CostBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    PriceBox.Focus(FocusState.Programmatic);
                }
            };

            PriceBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    ImageBox.Focus(FocusState.Programmatic);
                }
            };

            ImageBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    Save_Click(null, null);
                }
            };
        }

        // Seleccionar imagen
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
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "PuntoVenta",
                    "Products"
                );
                
                if (!Directory.Exists(assetsFolder))
                    Directory.CreateDirectory(assetsFolder);

                string newFileName = Guid.NewGuid() + Path.GetExtension(file.Name);
                string destinationPath = Path.Combine(assetsFolder, newFileName);

                using (var stream = await file.OpenStreamForReadAsync())
                using (var fileStream = File.Create(destinationPath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                selectedImagePath = destinationPath;
                ImageBox.Text = newFileName;
            }
        }

        private void OnlyNumbersDecimal_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            string text = args.NewText;

            if (text.Any(c => !char.IsDigit(c) && c != '.'))
            {
                args.Cancel = true;
                return;
            }

            if (text.Count(c => c == '.') > 1)
            {
                args.Cancel = true;
            }
        }

        // Guardar producto
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var products = await JsonService.LoadAsync<Product>("products.json");

            if (string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(BrandBox.Text) ||
                string.IsNullOrWhiteSpace(CostBox.Text) ||
                string.IsNullOrWhiteSpace(PriceBox.Text))
            {
                await ShowError("Todos los campos son obligatorios");
                return;
            }

            if (NameBox.Text.Trim().Length < 5)
            {
                await ShowError("El nombre del producto debe tener al menos 5 caracteres");
                return;
            }

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
                Existencias = 0,
                Imagen = selectedImagePath
            };

            products.Add(product);

            await JsonService.SaveAsync("products.json", products);

            this.Close();
        }

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