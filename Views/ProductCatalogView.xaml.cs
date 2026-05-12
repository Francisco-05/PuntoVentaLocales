using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using PuntoVenta.Helpers;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

namespace PuntoVenta.Views
{
    public sealed partial class ProductCatalogView : Page
    {
        private List<Product> products = new();

        private int currentPage = 1;
        private const int ProductsPerPage = 6;

        private readonly DispatcherTimer relojTimer = new();

        private Sale currentSale = new()
        {
            Details = new List<SaleDetail>(),
            Fecha = DateTime.Now
        };

        public ProductCatalogView()
        {
            InitializeComponent();

            UserText.Text =
                $"Empleado: {SessionService.CurrentUser?.NombreCompleto ?? "Sin sesión"}";

            LoadProducts();

            InitializeClock();
        }


        private void InitializeClock()
        {
            ClockText.Text = DateTime.Now.ToString("HH:mm");

            relojTimer.Interval = TimeSpan.FromSeconds(1);

            relojTimer.Tick += (s, e) =>
            {
                ClockText.Text = DateTime.Now.ToString("HH:mm");
            };

            relojTimer.Start();
        }
      
        

        

        private async void LoadProducts()
        {
            products =
                await JsonService.LoadAsync<Product>("products.json")
                ?? new List<Product>();

            currentPage = 1;

            ShowProductsPage();
        }

        private void ShowProductsPage()
        {
            var paginatedProducts = products
                .Skip((currentPage - 1) * ProductsPerPage)
                .Take(ProductsPerPage)
                .ToList();

            ProductsList.ItemsSource = paginatedProducts;

            PreviousPageButton.IsEnabled = currentPage > 1;
            NextPageButton.IsEnabled =
                currentPage * ProductsPerPage < products.Count;

            ProductsList.UpdateLayout();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage <= 1)
                return;

            currentPage--;

            ShowProductsPage();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage * ProductsPerPage >= products.Count)
                return;

            currentPage++;

            ShowProductsPage();
        }

        
        // Carrito
        
        private void RefreshCart()
        {
            CartList.ItemsSource = null;
            CartList.ItemsSource = currentSale.Details;

            TotalText.Text = $"Total: {currentSale.TotalBruto:C2}";
        }

        private int GetAvailableStock(int productId)
        {
            var product = products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
                return 0;

            var inCart = currentSale.Details
                .Where(d => d.ProductId == productId)
                .Sum(d => d.Cantidad);

            return Math.Max(0, product.Existencias - inCart);
        }

        private bool ProductoTieneExistencias(Product product) =>
            product != null && GetAvailableStock(product.Id) > 0;

        private void AgregarButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.DataContext is not Product product)
                return;

            button.IsEnabled = ProductoTieneExistencias(product);

            if (!button.IsEnabled)
            {
                button.Content = "Sin stock";
                button.Opacity = 0.6;
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button)?.DataContext as Product;

            if (product == null)
                return;

            var existing = currentSale.Details
                .FirstOrDefault(d => d.ProductId == product.Id);

            if (GetAvailableStock(product.Id) <= 0)
            {
                _ = ShowError(existing != null
                    ? "No hay existencias suficientes para agregar más unidades"
                    : "No hay existencias disponibles para este producto");

                return;
            }

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
            var item = (sender as Button)?.DataContext as SaleDetail;

            if (item == null)
                return;

            if (GetAvailableStock(item.ProductId) <= 0)
            {
                _ = ShowError("No hay existencias suficientes para agregar más unidades");
                return;
            }

            item.Cantidad++;

            RefreshCart();
        }

        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as SaleDetail;

            if (item == null)
                return;

            item.Cantidad--;

            if (item.Cantidad <= 0)
            {
                currentSale.Details.Remove(item);
            }

            RefreshCart();
        }

        //Validaciones
        private bool IsValidMoneyInput(string text)
        {
            if (text.Any(c => !char.IsDigit(c) && c != '.'))
                return false;

            if (text.Count(c => c == '.') > 1)
                return false;

            return !text.StartsWith(".");
        }
        private void Cantidad_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            // 🚫 Bloquear espacios
            if (args.NewText.Any(char.IsWhiteSpace))
            {
                args.Cancel = true;
                return;
            }

            // ✔ Permitir vacío (cuando el usuario borra con Backspace)
            if (string.IsNullOrEmpty(args.NewText))
                return;

            // ✔ Permitir solo números
            if (!int.TryParse(args.NewText, out int numero))
            {
                args.Cancel = true;
                return;
            }

            // ✔ No permitir negativos o cero
            if (numero <= 0)
            {
                args.Cancel = true;
                return;
            }

            var item = sender.DataContext as SaleDetail;
            if (item == null)
                return;

            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
                return;

            if (numero > product.Existencias)
            {
                args.Cancel = true;
            }
        }


        private async void Cantidad_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            var item = textBox.DataContext as SaleDetail;

            

            if (item == null)
                return;



            // ✔ vacío o inválido
            if (string.IsNullOrWhiteSpace(textBox.Text) ||
                !int.TryParse(textBox.Text, out int cantidad))
            {
                item.Cantidad = 1;
                textBox.Text = "1";

                if (!string.IsNullOrWhiteSpace(textBox.Text))
                    await ShowError("Solo se permiten números.");

                RefreshCart();
                return;
            }

            if (cantidad <= 0)
            {
                item.Cantidad = 1;
                textBox.Text = "1";

                await ShowError("La cantidad debe ser mayor a 0.");
                RefreshCart();
                return;
            }

            var product = products.FirstOrDefault(p => p.Id == item.ProductId);

            if (product == null)
                return;

            if (cantidad > product.Existencias)
            {
                item.Cantidad = product.Existencias;
                textBox.Text = product.Existencias.ToString();

                await ShowError($"Solo hay {product.Existencias} unidades disponibles.");
            }
            else
            {
                item.Cantidad = cantidad;
            }

            RefreshCart();
        }
        //COnfirmar compra

        private async void ConfirmSale_Click(object sender, RoutedEventArgs e)
        {
            if (!currentSale.Details.Any())
            {
                await ShowMessage(
                    "Carrito vacío",
                    "Agrega productos antes de confirmar la compra.");

                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Método de pago",
                Content = currentSale.TotalBruto > 100000
                    ? "Pagos mayores a $100,000 deben realizarse con tarjeta."
                    : "Selecciona cómo desea pagar el cliente",

                PrimaryButtonText = "Efectivo",
                SecondaryButtonText = "Tarjeta",
                CloseButtonText = "Cancelar",

                IsPrimaryButtonEnabled =
                    currentSale.TotalBruto <= 100000,

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
                    PlaceholderText = $"Total: {currentSale.TotalBruto:C2}",
                    MaxLength = 6
                };

                efectivoBox.BeforeTextChanging += (sender, args) =>
                {
                    string text = args.NewText;

                    // No permitir punto al inicio
                    if (text.StartsWith("."))
                    {
                        args.Cancel = true;
                        return;
                    }

                    // Solo permitir un punto decimal
                    if (text.Count(c => c == '.') > 1)
                    {
                        args.Cancel = true;
                        return;
                    }

                    // Solo números y punto decimal
                    if (text.Any(c => !char.IsDigit(c) && c != '.'))
                    {
                        args.Cancel = true;
                    }
                    if ( decimal.TryParse(text, out decimal value) &&
                        value > 100000
)
                    {
                        args.Cancel = true;
                    }
                };

                efectivoBox.TextChanging += (s, e) =>
                {
                    if (efectivoBox.Text.Contains(" "))
                    {
                        efectivoBox.Text =
                            efectivoBox.Text.Replace(" ", "");

                        efectivoBox.SelectionStart =
                            efectivoBox.Text.Length;
                    }
                };



                var cambioText = new TextBlock
                {
                    Text = "Cambio: $0.00",
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold
                };

                efectivoBox.BeforeTextChanging += (s, e2) =>
                {

                };

                var warningText = new TextBlock
                {
                    Text = "Monto máximo en efectivo: $100,000",
                    Foreground = new SolidColorBrush(Colors.Red),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var panel = new StackPanel();
                panel.Children.Add(efectivoBox);
                panel.Children.Add(warningText);
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
                    if (!double.TryParse(efectivoBox.Text, out double efectivo))
                    {
                        cambioText.Text = "Cambio: $0.00";
                        efectivoDialog.IsPrimaryButtonEnabled = false;
                        return;
                    }

                    double cambio = efectivo - currentSale.TotalBruto;

                    cambioText.Text = $"Cambio: {cambio:C2}";

                    cambioText.Foreground = cambio >= 0
                        ? new Microsoft.UI.Xaml.Media.SolidColorBrush(
                            Microsoft.UI.Colors.Green)
                        : new Microsoft.UI.Xaml.Media.SolidColorBrush(
                            Microsoft.UI.Colors.Red);

                    efectivoDialog.IsPrimaryButtonEnabled =
                        efectivo >= currentSale.TotalBruto;

                    if (efectivo >= currentSale.TotalBruto)
                    {
                        efectivoRecibidoFinal = efectivo;
                        cambioFinal = cambio;
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

            currentSale.Empleado =
                SessionService.CurrentUser?.NombreCompleto ?? "Desconocido";

            currentSale.Fecha = DateTime.Now;

            var updatedProducts =
                await JsonService.LoadAsync<Product>("products.json")
                ?? new List<Product>();

            foreach (var detail in currentSale.Details)
            {
                var product =
                    updatedProducts.FirstOrDefault(p => p.Id == detail.ProductId);

                if (product == null)
                {
                    await ShowError("Producto no encontrado en el inventario");
                    return;
                }

                if (product.Existencias < detail.Cantidad)
                {
                    await ShowError(
                        "No hay existencias suficientes para completar la venta");

                    return;
                }
            }

            foreach (var detail in currentSale.Details)
            {
                var product =
                    updatedProducts.First(p => p.Id == detail.ProductId);

                product.Existencias -= detail.Cantidad;
            }

            await JsonService.SaveAsync("products.json", updatedProducts);

            products = updatedProducts;

            ShowProductsPage();

            await SaleService.AddAsync(currentSale);

            string mensaje =
                $"Total: {currentSale.TotalBruto:C2}\n" +
                $"Pago: {currentSale.MetodoPago}";

            if (currentSale.MetodoPago == "Efectivo")
            {
                mensaje +=
                    $"\nEfectivo recibido: {efectivoRecibidoFinal:C2}" +
                    $"\nCambio: {cambioFinal:C2}";
            }

            await ShowMessage("Venta realizada", mensaje);

            currentSale = new Sale
            {
                Details = new List<SaleDetail>(),
                Fecha = DateTime.Now
            };

            RefreshCart();
        }

        // Corte de caja
        
        private async void CashCut_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = SessionService.CurrentUser;

            if (currentUser == null)
            {
                await ShowError("No hay usuario en sesión.");
                return;
            }

            var passwordBox = new PasswordBox {
                MaxLength = 15,
                Padding = new Thickness(40, 6, 0, 0)
            };

            passwordBox.PasswordChanged += (s, e) =>
            {
                if (passwordBox.Password.Contains(" "))
                {
                    passwordBox.Password =
                        passwordBox.Password.Replace(" ", "");
                }
            };

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
                authDialog.IsPrimaryButtonEnabled =
                    !string.IsNullOrWhiteSpace(passwordBox.Password);
            };

            var authResult = await authDialog.ShowAsync();

            if (authResult != ContentDialogResult.Primary)
                return;

            var user =
                await UserService.Login(
                    currentUser.Username,
                    passwordBox.Password);

            if (user == null)
            {
                await ShowError("Contraseña incorrecta");
                return;
            }

            var sales =
                await JsonService.LoadAsync<Sale>("sales.json")
                ?? new List<Sale>();

            var inicio = SessionService.LoginTime;
            var fin = DateTime.Now;

            var ventasSesion = sales
                .Where(s =>
                    s.Fecha >= inicio &&
                    s.Fecha <= fin &&
                    s.Empleado == currentUser.NombreCompleto)
                .ToList();

            double totalBruto = ventasSesion.Sum(v => v.TotalBruto);

            double utilidad = ventasSesion.Sum(v => v.Utilidad);

            double efectivoSistema = ventasSesion
                .Where(v => v.MetodoPago == "Efectivo")
                .Sum(v => v.TotalBruto);

            var efectivoBox = new TextBox
            {
                PlaceholderText = "Ingrese efectivo en caja",
                MaxLength = 10,
            };

            efectivoBox.TextChanging += (s, e) =>
            {
                if (efectivoBox.Text.Contains(" "))
                {
                    efectivoBox.Text =
                        efectivoBox.Text.Replace(" ", "");

                    efectivoBox.SelectionStart =
                        efectivoBox.Text.Length;
                }
            };

            efectivoBox.BeforeTextChanging += (sender, args) =>
            {
                string text = args.NewText;

                // Solo números y un punto decimal
                bool valid =
                    text.Count(c => c == '.') <= 1 &&
                    text.All(c => char.IsDigit(c) || c == '.');

                args.Cancel = !valid;
            };

            efectivoBox.TextChanging += (s, e) =>
            {
                InputValidationHelper.PreventLeadingSpaces(efectivoBox);
            };

            efectivoBox.BeforeTextChanging += (s, e2) =>
            {
                e2.Cancel = !IsValidMoneyInput(e2.NewText);
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

            if (!double.TryParse(
                    efectivoBox.Text,
                    out double efectivoRealFinal))
            {
                await ShowError("Debes ingresar el efectivo en caja.");
                return;
            }

            double diferenciaFinal =
                efectivoRealFinal - efectivoSistema;

            var diferencias =
                await JsonService.LoadAsync<DiferenciaCaja>(
                    "diferenciasCaja.json")
                ?? new List<DiferenciaCaja>();

            var nuevaDiferencia = new DiferenciaCaja
            {
                Id = diferencias.Count > 0
                    ? diferencias.Max(d => d.Id) + 1
                    : 1,

                Empleado = currentUser.NombreCompleto,
                Fecha = DateTime.Now,
                InicioSesion = inicio,
                FinSesion = fin,
                EfectivoSistema = efectivoSistema,
                EfectivoReal = efectivoRealFinal,
                Diferencia = diferenciaFinal
            };

            diferencias.Add(nuevaDiferencia);

            await JsonService.SaveAsync(
                "diferenciasCaja.json",
                diferencias);

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

            await ShowMessage("Reporte de corte", reporte);

            SessionService.CurrentUser = null;
            SessionService.LoginTime = DateTime.MinValue;

            MainWindow.Instance.MainFrameControl
                .Navigate(typeof(LoginView));
        }



        private async Task ShowError(string message)
        {
            await ShowMessage("Error", message);
        }

        private async Task ShowMessage(string title, string message)
        {
            await new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();
        }
    }
}