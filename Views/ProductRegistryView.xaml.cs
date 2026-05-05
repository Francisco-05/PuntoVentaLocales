using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System.Collections.Generic;

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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(AdminView));
        }
    }
}
