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
        private List<Product> products = new List<Product>();

        private int currentPage = 1;
        private int productsPerPage = 6;

        // Venta actual (carrito)
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

        // Cargar productos desde JSON
        private async void LoadProducts()
        {
            products = await JsonService.LoadAsync<Product>("products.json") ?? new List<Product>();
            currentPage = 1;
            ShowProductsPage();
        }

        // Mostrar productos por página
        private void ShowProductsPage()
        {
            var paginatedProducts = products
                .Skip((currentPage - 1) * productsPerPage)
                .Take(productsPerPage)
                .ToList();

            ProductsList.ItemsSource = paginatedProducts;

            PreviousPageButton.IsEnabled = currentPage > 1;
            NextPageButton.IsEnabled = currentPage * productsPerPage < products.Count;
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                ShowProductsPage();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage * productsPerPage < products.Count)
            {
                currentPage++;
                ShowProductsPage();
            }
        }

        // Refrescar vista del carrito
        private void RefreshCart()
        {
            CartList.ItemsSource = null;
            CartList.ItemsSource = currentSale.Details;

            TotalText.Text = $"Total: {currentSale.TotalBruto:C2}";
        }

        private async Task ShowError(string message)
        {
            await new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();
        }

        private int GetAvailableStock(int productId)
        {
            var product = products?.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return 0;
            }

            var inCart = currentSale.Details
                .Where(d => d.ProductId == productId)
                .Sum(d => d.Cantidad);

            return Math.Max(0, product.Existencias - inCart);
        }

        // Agregar producto al carrito
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button)?.DataContext as Product;
            if (product == null) return;

            var existing = currentSale.Details
                .FirstOrDefault(d => d.ProductId == product.Id);

            if (existing != null)
            {
                if (GetAvailableStock(product.Id) <= 0)
                {
                    _ = ShowError("No hay existencias suficientes para agregar más unidades");
                    return;
                }

                existing.Cantidad++;
            }
            else
            {
                if (GetAvailableStock(product.Id) <= 0)
                {
                    _ = ShowError("No hay existencias disponibles para este producto");
                    return;
                }

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

        // Aumentar cantidad desde el carrito
        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as SaleDetail;
            if (item == null) return;

            if (GetAvailableStock(item.ProductId) <= 0)
            {
                _ = ShowError("No hay existencias suficientes para agregar más unidades");
                return;
            }

            item.Cantidad++;
            RefreshCart();
        }

        // Disminuir cantidad o eliminar del carrito
        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as SaleDetail;
            if (item == null) return;

            item.Cantidad--;

            if (item.Cantidad <= 0)
            {
                currentSale.Details.Remove(item);
            }

            RefreshCart();
        }

        // Confirmar venta
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

                    if (text.StartsWith("."))
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

            var updatedProducts = await JsonService.LoadAsync<Product>("products.json") ?? new List<Product>();

            foreach (var detail in currentSale.Details)
            {
                var product = updatedProducts.FirstOrDefault(p => p.Id == detail.ProductId);
                if (product == null)
                {
                    await ShowError("Producto no encontrado en el inventario");
                    return;
                }

                if (product.Existencias < detail.Cantidad)
                {
                    await ShowError("No hay existencias suficientes para completar la venta");
                    return;
                }
            }

            foreach (var detail in currentSale.Details)
            {
                var product = updatedProducts.First(p => p.Id == detail.ProductId);
                product.Existencias -= detail.Cantidad;
            }

            await JsonService.SaveAsync("products.json", updatedProducts);
            products = updatedProducts;
            ShowProductsPage();

            await SaleService.AddAsync(currentSale);

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

            if (currentUser == null)
            {
                await ShowError("No hay usuario en sesión.");
                return;
            }

            var passwordBox = new PasswordBox();

            var authDialog = new ContentDialog
            {
                Title = "Confirmar identidad",
                Content = passwordBox,
                PrimaryButtonText = "Continuar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.XamlRoot,
                IsPrimaryButtonEnabled = false
            };

            passwordBox.PasswordChanged += (s, ev) =>
            {
                authDialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(passwordBox.Password);
            };

            var authResult = await authDialog.ShowAsync();

            if (authResult != ContentDialogResult.Primary)
                return;

            var user = await UserService.Login(currentUser.Username, passwordBox.Password);

            if (user == null)
            {
                await ShowError("Contraseña incorrecta");
                return;
            }

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

            var efectivoBox = new TextBox
            {
                PlaceholderText = "Ingrese efectivo en caja"
            };

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

                if (text.StartsWith("."))
                {
                    e2.Cancel = true;
                }
            };

            var panel = new StackPanel();
            panel.Children.Add(efectivoBox);

            var cashDialog = new ContentDialog
            {
                Title = "Corte de caja",
                Content = panel,
                PrimaryButtonText = "Confirmar corte",
                CloseButtonText = "Cancelar",
                XamlRoot = this.XamlRoot,
                IsPrimaryButtonEnabled = false
            };

            efectivoBox.TextChanged += (s, ev) =>
            {
                cashDialog.IsPrimaryButtonEnabled =
                    !string.IsNullOrWhiteSpace(efectivoBox.Text) &&
                    double.TryParse(efectivoBox.Text, out double efectivoReal) &&
                    efectivoReal >= 0;
            };

            var cashResult = await cashDialog.ShowAsync();

            if (cashResult != ContentDialogResult.Primary)
                return;

            if (!double.TryParse(efectivoBox.Text, out double efectivoRealFinal))
            {
                await ShowError("Debes ingresar el efectivo en caja.");
                return;
            }

            double diferenciaFinal = efectivoRealFinal - efectivoSistema;

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
                EfectivoReal = efectivoRealFinal,
                Diferencia = diferenciaFinal
            };

            diferencias.Add(nuevaDiferencia);

            await JsonService.SaveAsync("diferenciasCaja.json", diferencias);

            string reporte =
                $"Empleado: {currentUser.NombreCompleto}\n" +
                $"Inicio: {inicio}\n" +
                $"Corte: {fin}\n\n" +
                $"Ventas: {ventasSesion.Count}\n" +
                $"Total vendido: {totalBruto:C2}\n" +
                $"Utilidad: {utilidad:C2}\n\n" +
                $"Efectivo en caja (Sistema): {efectivoSistema:C2}\n" +
                $"Efectivo en caja (Físico): {efectivoRealFinal:C2}\n" +
                $"Diferencia: {diferenciaFinal:C2}";

            await new ContentDialog
            {
                Title = "Reporte de corte",
                Content = reporte,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();

            SessionService.CurrentUser = null;
            SessionService.LoginTime = DateTime.MinValue;

            MainWindow.Instance.MainFrameControl.Navigate(typeof(LoginView));
        }
    }
}