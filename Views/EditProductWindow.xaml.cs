using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace PuntoVenta.Views
{
    public sealed partial class EditProductWindow : Window
    {
        private readonly Product product;
        private string selectedImagePath = "";

        public EditProductWindow(Product product)
        {
            this.InitializeComponent();
            this.product = product;
            LoadProduct();
        }

        private void LoadProduct()
        {
            NameBox.Text = product.Nombre;
            BrandBox.Text = product.Marca;
            DescBox.Text = product.Descripcion;
            CostBox.Text = product.Costo.ToString();
            PriceBox.Text = product.PrecioVenta.ToString();
            StockBox.Text = product.Existencias.ToString();
            selectedImagePath = product.Imagen;
            ImageBox.Text = Path.GetFileName(product.Imagen);
        }

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

        private void OnlyNumbers_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            string text = args.NewText;

            if (text.Any(c => !char.IsDigit(c)))
            {
                args.Cancel = true;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var products = await JsonService.LoadAsync<Product>("products.json");

            if (string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(BrandBox.Text) ||
                string.IsNullOrWhiteSpace(CostBox.Text) ||
                string.IsNullOrWhiteSpace(PriceBox.Text) ||
                string.IsNullOrWhiteSpace(StockBox.Text))
            {
                await ShowError("Todos los campos son obligatorios");
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

            if (!int.TryParse(StockBox.Text, out int existencias))
            {
                await ShowError("Existencias inválidas");
                return;
            }

            if (costo < 0 || precio < 0)
            {
                await ShowError("No se permiten valores negativos");
                return;
            }

            if (existencias < 0)
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
                p.Id != product.Id &&
                p.Nombre.ToLower() == NameBox.Text.ToLower() &&
                p.Marca.ToLower() == BrandBox.Text.ToLower()
            );

            if (exists)
            {
                await ShowError("Ya existe un producto con ese nombre y marca");
                return;
            }

            var index = products.FindIndex(p => p.Id == product.Id);
            if (index < 0)
            {
                await ShowError("Producto no encontrado");
                return;
            }

            product.Nombre = NameBox.Text;
            product.Marca = BrandBox.Text;
            product.Descripcion = DescBox.Text;
            product.Costo = costo;
            product.PrecioVenta = precio;
            product.Existencias = existencias;
            product.Imagen = selectedImagePath;

            products[index] = product;

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
