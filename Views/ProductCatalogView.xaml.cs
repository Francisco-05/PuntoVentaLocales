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

        // Venta actual (carrito)
        private Sale currentSale = new Sale
        {
            Details = new List<SaleDetail>(),
            Fecha = DateTime.Now
        };

        public ProductCatalogView()
        {
            this.InitializeComponent();
            // Mostrar nombre del empleado en la parte superior

            UserText.Text = $"Empleado: {SessionService.CurrentUser?.NombreCompleto ?? "Sin sesión"}";

            LoadProducts();
        }


        private async void LoadProducts()
        {
            // Cargar productos desde JSON
            products = await JsonService.LoadAsync<Product>("products.json");
            ProductsList.ItemsSource = products;
        }

        // Refrescar la lista del carrito y el total
        private void RefreshCart()
        {
            CartList.ItemsSource = null;
            CartList.ItemsSource = currentSale.Details;

            TotalText.Text = $"Total: {currentSale.TotalBruto:C2}";
        }

        // Agregar producto al carrito
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el producto asociado al botón
            var product = (sender as Button).DataContext as Product;
            if (product == null) return;

            var existing = currentSale.Details
                .FirstOrDefault(d => d.ProductId == product.Id);

            // Si ya está en el carrito, aumentar cantidad, sino agregar nuevo detalle
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

        // Aumentar o disminuir cantidad desde el carrito
        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as SaleDetail;
            if (item == null) return;

            item.Cantidad++;
            RefreshCart();
        }

        // Disminuir cantidad o eliminar del carrito
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

        // Confirmar venta y elegir método de pago
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

            // Efectivo: pedir monto, calcular cambio, validar que sea suficiente
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

                // Validar que solo se ingresen números y un punto decimal
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

                // Panel para mostrar ambos controles juntos
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

                // Calcular cambio en tiempo real y validar monto
                efectivoBox.TextChanged += (s, ev) =>
                {
                    if (double.TryParse(efectivoBox.Text, out double efectivo))
                    {
                        double cambio = efectivo - currentSale.TotalBruto;
                        cambioText.Text = $"Cambio: {cambio:C2}";
                        // Cambiar color del texto según si el efectivo es suficiente o no
                        cambioText.Foreground = cambio >= 0
                            ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
                            : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);

                        efectivoDialog.IsPrimaryButtonEnabled = efectivo >= currentSale.TotalBruto;

                        // Guardar valores finales para mostrar en el mensaje final
                        if (efectivo >= currentSale.TotalBruto)
                        {
                            efectivoRecibidoFinal = efectivo;
                            cambioFinal = cambio;
                        }
                    }
                    else
                    {
                        // Si no es un número válido, deshabilitar el botón y mostrar cambio como $0.00
                        cambioText.Text = "Cambio: $0.00";
                        efectivoDialog.IsPrimaryButtonEnabled = false;
                    }
                };

                var efectivoResult = await efectivoDialog.ShowAsync();
                // Si el usuario cancela el diálogo de efectivo, no proceder con la venta
                if (efectivoResult != ContentDialogResult.Primary)
                    return;
            }
            else
            {
                currentSale.MetodoPago = "Tarjeta";
            }

            // Guardar venta
            currentSale.Empleado = SessionService.CurrentUser?.NombreCompleto ?? "Desconocido";
            currentSale.Fecha = DateTime.Now;

            await SaleService.AddAsync(currentSale);

          
            string mensaje = $"Total: {currentSale.TotalBruto:C2}\nPago: {currentSale.MetodoPago}";

            // Solo mostrar detalles de efectivo si se pagó en efectivo
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
        // Corte de caja: pedir contraseña, validar, mostrar ventas del día, pedir efectivo en caja, calcular diferencia, guardar reporte
        private async void CashCut_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = SessionService.CurrentUser;

            var passwordBox = new PasswordBox();

            var authDialog = new ContentDialog
            {
                Title = "Confirmar identidad (ingresa tu contraseña)",
                Content = passwordBox,
                PrimaryButtonText = "Continuar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.XamlRoot
            };

            var authResult = await authDialog.ShowAsync();

            if (authResult != ContentDialogResult.Primary)
                return;

            // Validar contraseña
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

            // Cargar ventas del día para este empleado
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

            // Pedir efectivo real en caja
            var efectivoBox = new TextBox
            {
                PlaceholderText = "Ingrese efectivo en caja"
            };

            // solo números y un punto decimal
            efectivoBox.BeforeTextChanging += (s, e2) =>
            {
                string text = e2.NewText;

                // Solo permitir números y punto
                if (text.Any(c => !char.IsDigit(c) && c != '.'))
                {
                    e2.Cancel = true;
                    return;
                }

                // Solo permitir un punto decimal
                if (text.Count(c => c == '.') > 1)
                {
                    e2.Cancel = true;
                }

                // Evitar que empiece con punto
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
                XamlRoot = this.XamlRoot
            };

            var cashResult = await cashDialog.ShowAsync();

            // Si el usuario cancela el diálogo, no proceder con el corte
            if (cashResult != ContentDialogResult.Primary)
                return;

            // Validar número final
            if (!double.TryParse(efectivoBox.Text, out double efectivoReal))
                efectivoReal = 0;

            double diferenciaFinal = efectivoReal - efectivoSistema;

            // Guardar diferencia en JSON
            var diferencias = await JsonService.LoadAsync<DiferenciaCaja>("diferenciasCaja.json")
                              ?? new List<DiferenciaCaja>();

            // Asignar un ID incremental
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

            // Cerrar sesión
            SessionService.CurrentUser = null;
            SessionService.LoginTime = DateTime.MinValue;

            // Volver al login
            MainWindow.Instance.MainFrameControl.Navigate(typeof(LoginView));
        }

    }
}