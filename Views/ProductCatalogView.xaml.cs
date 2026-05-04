using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace PuntoVenta.Views
{
    public sealed partial class ProductCatalogView : Page
    {
        private List<Product> products;

        // 🛒 Venta actual (carrito)
        private Sale currentSale = new Sale
        {
            Details = new List<SaleDetail>(),
            Fecha = DateTime.Now
        };

        public ProductCatalogView()
        {
            this.InitializeComponent();

            // 🔥 MOSTRAR USUARIO LOGUEADO
            UserText.Text = $"Empleado: {SessionService.CurrentUser?.NombreCompleto ?? "Sin sesión"}";

            LoadProducts();
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(LoginView));
        }

        // 📦 CARGAR PRODUCTOS
        private async void LoadProducts()
        {
            products = await JsonService.LoadAsync<Product>("products.json");
            ProductsList.ItemsSource = products;
        }

        // 🔄 REFRESCAR CARRITO (UI)
        private void RefreshCart()
        {
            CartList.ItemsSource = null;
            CartList.ItemsSource = currentSale.Details;

            TotalText.Text = $"Total: {currentSale.TotalBruto:C2}";
        }

        // 🛒 AGREGAR AL CARRITO
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button).DataContext as Product;
            if (product == null) return;

            var existing = currentSale.Details
                .FirstOrDefault(d => d.ProductId == product.Id);

            if (existing != null)
            {
                existing.Cantidad++;
            }
            else
            {
                currentSale.Details.Add(new SaleDetail
                {
                    ProductId = product.Id,
                    Nombre = product.Nombre,
                    Marca = product.Marca,
                    PrecioUnitario = product.PrecioVenta,
                    CostoUnitario = product.Costo,
                    Cantidad = 1
                });
            }

            RefreshCart();
        }

        // ➕ AUMENTAR CANTIDAD
        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as SaleDetail;
            if (item == null) return;

            item.Cantidad++;
            RefreshCart();
        }

        // ➖ DISMINUIR / ELIMINAR
        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as SaleDetail;
            if (item == null) return;

            item.Cantidad--;

            if (item.Cantidad <= 0)
            {
                currentSale.Details.Remove(item);
            }

            RefreshCart();
        }

        // 💳 CONFIRMAR COMPRA
        private async void ConfirmSale_Click(object sender, RoutedEventArgs e)
        {
            if (!currentSale.Details.Any())
            {
                await new ContentDialog
                {
                    Title = "Carrito vacío",
                    Content = "Agrega productos antes de confirmar la compra.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
                return;
            }

            // 💳 Método de pago
            var dialog = new ContentDialog
            {
                Title = "Método de pago",
                Content = "Selecciona cómo desea pagar el cliente",
                PrimaryButtonText = "Efectivo",
                SecondaryButtonText = "Tarjeta",
                CloseButtonText = "Cancelar",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.None)
                return;

            currentSale.MetodoPago = result == ContentDialogResult.Primary
                ? "Efectivo"
                : "Tarjeta";

            // 👤 EMPLEADO
            currentSale.Empleado = SessionService.CurrentUser?.NombreCompleto ?? "Desconocido";

            currentSale.Fecha = DateTime.Now;

            // 🔥 GUARDAR VENTA (USANDO SERVICE)
            await SaleService.AddAsync(currentSale);

            // ✅ Confirmación
            await new ContentDialog
            {
                Title = "Venta realizada",
                Content = $"Total: {currentSale.TotalBruto:C2}\nPago: {currentSale.MetodoPago}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();

            // 🔄 Limpiar carrito
            currentSale = new Sale
            {
                Details = new List<SaleDetail>(),
                Fecha = DateTime.Now
            };

            RefreshCart();
        }
    }
}