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

            UserText.Text = $"Empleado: {SessionService.CurrentUser?.NombreCompleto ?? "Sin sesión"}";

            LoadProducts();
        }


        private async void LoadProducts()
        {
            products = await JsonService.LoadAsync<Product>("products.json");
            ProductsList.ItemsSource = products;
        }

        private void RefreshCart()
        {
            CartList.ItemsSource = null;
            CartList.ItemsSource = currentSale.Details;

            TotalText.Text = $"Total: {currentSale.TotalBruto:C2}";
        }

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

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as SaleDetail;
            if (item == null) return;

            item.Cantidad++;
            RefreshCart();
        }

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

        // 💳 CONFIRMAR COMPRA (YA CON EFECTIVO)
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

            double efectivoRecibidoFinal = 0;
            double cambioFinal = 0;

            // 💵 EFECTIVO
            if (result == ContentDialogResult.Primary)
            {
                currentSale.MetodoPago = "Efectivo";

                var efectivoBox = new TextBox
                {
                    PlaceholderText = $"Total: {currentSale.TotalBruto:C2}"
                };

                var cambioText = new TextBlock
                {
                    Text = "Cambio: $0.00",
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold
                };

                // 🔒 SOLO NÚMEROS Y PUNTO
                efectivoBox.BeforeTextChanging += (s, e2) =>
                {
                    string text = e2.NewText;

                    if (text.Any(c => !char.IsDigit(c) && c != '.'))
                    {
                        e2.Cancel = true;
                        return;
                    }

                    if (text.Count(c => c == '.') > 1)
                    {
                        e2.Cancel = true;
                    }
                };

                var panel = new StackPanel();
                panel.Children.Add(efectivoBox);
                panel.Children.Add(cambioText);

                var efectivoDialog = new ContentDialog
                {
                    Title = "Pago en efectivo",
                    Content = panel,
                    PrimaryButtonText = "Confirmar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.XamlRoot,
                    IsPrimaryButtonEnabled = false
                };

                // 🔄 CAMBIO + CONTROL BOTÓN
                efectivoBox.TextChanged += (s, ev) =>
                {
                    if (double.TryParse(efectivoBox.Text, out double efectivo))
                    {
                        double cambio = efectivo - currentSale.TotalBruto;
                        cambioText.Text = $"Cambio: {cambio:C2}";

                        cambioText.Foreground = cambio >= 0
                            ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
                            : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);

                        efectivoDialog.IsPrimaryButtonEnabled = efectivo >= currentSale.TotalBruto;

                        // 🔥 guardar directo aquí (ya válido)
                        if (efectivo >= currentSale.TotalBruto)
                        {
                            efectivoRecibidoFinal = efectivo;
                            cambioFinal = cambio;
                        }
                    }
                    else
                    {
                        cambioText.Text = "Cambio: $0.00";
                        efectivoDialog.IsPrimaryButtonEnabled = false;
                    }
                };

                var efectivoResult = await efectivoDialog.ShowAsync();

                if (efectivoResult != ContentDialogResult.Primary)
                    return;
            }
            else
            {
                currentSale.MetodoPago = "Tarjeta";
            }

            currentSale.Empleado = SessionService.CurrentUser?.NombreCompleto ?? "Desconocido";
            currentSale.Fecha = DateTime.Now;

            await SaleService.AddAsync(currentSale);

            // 🔥 MENSAJE FINAL
            string mensaje = $"Total: {currentSale.TotalBruto:C2}\nPago: {currentSale.MetodoPago}";

            if (currentSale.MetodoPago == "Efectivo")
            {
                mensaje += $"\nEfectivo recibido: {efectivoRecibidoFinal:C2}" +
                           $"\nCambio: {cambioFinal:C2}";
            }

            await new ContentDialog
            {
                Title = "Venta realizada",
                Content = mensaje,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();

            currentSale = new Sale
            {
                Details = new List<SaleDetail>(),
                Fecha = DateTime.Now
            };

            RefreshCart();
        }
        private async void CashCut_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = SessionService.CurrentUser;

            // 🔐 PEDIR CONTRASEÑA
            var passwordBox = new PasswordBox();

            var authDialog = new ContentDialog
            {
                Title = "Confirmar identidad",
                Content = passwordBox,
                PrimaryButtonText = "Continuar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.XamlRoot
            };

            var authResult = await authDialog.ShowAsync();

            if (authResult != ContentDialogResult.Primary)
                return;

            // 🔥 VALIDAR LOGIN OTRA VEZ
            var user = await UserService.Login(currentUser.Username, passwordBox.Password);

            if (user == null)
            {
                await new ContentDialog
                {
                    Title = "Error",
                    Content = "Contraseña incorrecta",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
                return;
            }

            // 📊 CARGAR VENTAS
            var sales = await JsonService.LoadAsync<Sale>("sales.json") ?? new List<Sale>();

            var inicio = SessionService.LoginTime;
            var fin = DateTime.Now;

            var ventasSesion = sales
                .Where(s => s.Fecha >= inicio && s.Fecha <= fin &&
                            s.Empleado == currentUser.NombreCompleto)
                .ToList();

            double totalBruto = ventasSesion.Sum(v => v.TotalBruto);
            double utilidad = ventasSesion.Sum(v => v.Utilidad);

            double efectivoSistema = ventasSesion
                .Where(v => v.MetodoPago == "Efectivo")
                .Sum(v => v.TotalBruto);

            // 💵 PEDIR EFECTIVO EN CAJA
            var efectivoBox = new TextBox
            {
                PlaceholderText = "Ingrese efectivo en caja"
            };

            var panel = new StackPanel();
            panel.Children.Add(efectivoBox);

            var cashDialog = new ContentDialog
            {
                Title = "Corte de caja",
                Content = panel,
                PrimaryButtonText = "Confirmar corte",
                CloseButtonText = "Cancelar",
                XamlRoot = this.XamlRoot
            };

            var cashResult = await cashDialog.ShowAsync();

            if (cashResult != ContentDialogResult.Primary)
                return;

            // 🔥 VALIDACIÓN SEGURA
            if (!double.TryParse(efectivoBox.Text, out double efectivoReal))
                efectivoReal = 0;

            double diferenciaFinal = efectivoReal - efectivoSistema;

            // 📁 GUARDAR DIFERENCIA
            var diferencias = await JsonService.LoadAsync<DiferenciaCaja>("diferenciasCaja.json")
                              ?? new List<DiferenciaCaja>();

            var nuevaDiferencia = new DiferenciaCaja
            {
                Id = diferencias.Count > 0 ? diferencias.Max(d => d.Id) + 1 : 1,
                Empleado = currentUser.NombreCompleto,
                Fecha = DateTime.Now,
                InicioSesion = inicio,
                FinSesion = fin,
                EfectivoSistema = efectivoSistema,
                EfectivoReal = efectivoReal,
                Diferencia = diferenciaFinal
            };

            diferencias.Add(nuevaDiferencia);

            await JsonService.SaveAsync("diferenciasCaja.json", diferencias);

            // 🧾 REPORTE FINAL
            string reporte =
                $"Empleado: {currentUser.NombreCompleto}\n" +
                $"Inicio: {inicio}\n" +
                $"Corte: {fin}\n\n" +
                $"Ventas: {ventasSesion.Count}\n" +
                $"Total vendido: {totalBruto:C2}\n" +
                $"Utilidad: {utilidad:C2}\n\n" +
                $"Efectivo en caja (Sistema): {efectivoSistema:C2}\n" +
                $"Efectivo en caja (Físico): {efectivoReal:C2}\n" +
                $"Diferencia: {diferenciaFinal:C2}";

            await new ContentDialog
            {
                Title = "Reporte de corte",
                Content = reporte,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();

            // 🔒 LIMPIAR SESIÓN
            SessionService.CurrentUser = null;
            SessionService.LoginTime = DateTime.MinValue;

            // 🔁 REGRESAR AL LOGIN (LOGOUT)
            MainWindow.Instance.MainFrameControl.Navigate(typeof(LoginView));
        }

    }
}