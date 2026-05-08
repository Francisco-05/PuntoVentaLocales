using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PuntoVenta.Views
{
    public sealed partial class ProductRegistryView : Page
    {
        private List<Product> products = new();

        public ProductRegistryView()
        {
            this.InitializeComponent();
            LoadProducts();
        }

        private async void LoadProducts()
        {
            products = await JsonService.LoadAsync<Product>("products.json");
            ProductsList.ItemsSource = products;
        }

        private void Modify_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Product product)
            {
                var win = new EditProductWindow(product);
                win.Activate();
            }
        }

        private async void Restock_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not Product product)
            {
                return;
            }

            var amountBox = new TextBox
            {
                PlaceholderText = "Cantidad a añadir"
            };

            amountBox.BeforeTextChanging += (s, e2) =>
            {
                string text = e2.NewText;

                if (text.Any(c => !char.IsDigit(c)))
                {
                    e2.Cancel = true;
                }
            };

            var dialog = new ContentDialog
            {
                Title = "Reabastecimiento",
                Content = amountBox,
                PrimaryButtonText = "Agregar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            if (!int.TryParse(amountBox.Text, out int amount) || amount <= 0)
            {
                await new ContentDialog
                {
                    Title = "Error",
                    Content = "Ingresa una cantidad válida mayor a 0",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
                return;
            }

            product.Existencias += amount;

            await JsonService.SaveAsync("products.json", products);

            ProductsList.ItemsSource = null;
            ProductsList.ItemsSource = products;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not Product product)
            {
                return;
            }

            var confirmDialog = new ContentDialog
            {
                Title = "Eliminar producto",
                Content = $"¿Deseas eliminar el producto {product.Nombre}?",
                PrimaryButtonText = "Eliminar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            products.Remove(product);
            await JsonService.SaveAsync("products.json", products);

            ProductsList.ItemsSource = null;
            ProductsList.ItemsSource = products;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(AdminView));
        }
    }
}
