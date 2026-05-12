using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Helpers;
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
            InitializeComponent();

            this.product = product;

            LoadProduct();

            // Bloquear spam por tecla mantenida
            NameBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;
            BrandBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;
            DescBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;
            CostBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;
            PriceBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;
            StockBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;
            ImageBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;

            // Nombre - no permitir espacios al inicio ni dobles espacios
            NameBox.TextChanging += (s, e) =>
            {
                CleanTextBoxSpaces(NameBox);
            };

            // Marca - no permitir espacios al inicio ni dobles espacios
            BrandBox.TextChanging += (s, e) =>
            {
                CleanTextBoxSpaces(BrandBox);
            };

            // Descripción - no permitir espacios al inicio ni dobles espacios
            DescBox.TextChanging += (s, e) =>
            {
                CleanTextBoxSpaces(DescBox);
            };

            CostBox.TextChanging += (s, e) =>
            {
                InputValidationHelper.PreventLeadingSpaces(CostBox);
            };

            PriceBox.TextChanging += (s, e) =>
            {
                InputValidationHelper.PreventLeadingSpaces(PriceBox);
            };

            StockBox.TextChanging += (s, e) =>
            {
                InputValidationHelper.PreventLeadingSpaces(StockBox);
            };

            ImageBox.TextChanging += (s, e) =>
            {
                InputValidationHelper.PreventLeadingSpaces(ImageBox);
            };

            SetupEnterNavigation();
        }

        // Configura la navegación con Enter entre los campos y el botón de guardar
        private void SetupEnterNavigation()
        {
            SetEnterFocus(NameBox, BrandBox);
            SetEnterFocus(BrandBox, DescBox);
            SetEnterFocus(DescBox, CostBox);
            SetEnterFocus(CostBox, PriceBox);
            SetEnterFocus(PriceBox, StockBox);
            SetEnterFocus(StockBox, ImageBox);

            ImageBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    Save_Click(null, null);
                }
            };
        }


        private void SetEnterFocus(Control current, Control next)
        {
            current.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    next.Focus(FocusState.Programmatic);
                }
            };
        }

        // Elimina espacios al inicio y dobles espacios dentro del texto de un TextBox
        private void CleanTextBoxSpaces(TextBox box)
        {
            int cursorPosition = box.SelectionStart;

            string nuevoTexto = box.Text.TrimStart();

            while (nuevoTexto.Contains("  "))
            {
                nuevoTexto = nuevoTexto.Replace("  ", " ");
            }

            if (box.Text != nuevoTexto)
            {
                box.Text = nuevoTexto;

                if (cursorPosition > 0)
                {
                    box.SelectionStart = Math.Min(cursorPosition - 1, box.Text.Length);
                }
            }
        }

        // Carga los datos del producto en los campos del formulario
        private void LoadProduct()
        {
            NameBox.Text = product.Nombre;
            BrandBox.Text = product.Marca;
            DescBox.Text = product.Descripcion;
            CostBox.Text = product.Costo.ToString();
            PriceBox.Text = product.PrecioVenta.ToString();
            StockBox.Text = product.Existencias.ToString();

            selectedImagePath = product.Imagen;

            ImageBox.Text =
                Path.GetFileName(product.Imagen);
        }

        // Permite seleccionar una imagen para el producto y la guarda en la carpeta de productos

        private async void SelectImage_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            var picker = new FileOpenPicker();

            InitializeWithWindow.Initialize(
                picker,
                WindowNative.GetWindowHandle(this)
            );

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile file =
                await picker.PickSingleFileAsync();

            if (file == null)
                return;

            string folder = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments
                ),
                "PuntoVenta",
                "Products"
            );

            Directory.CreateDirectory(folder);

            string fileName =
                Guid.NewGuid() +
                Path.GetExtension(file.Name);

            string destination =
                Path.Combine(folder, fileName);

            using var stream =
                await file.OpenStreamForReadAsync();

            using var fileStream =
                File.Create(destination);

            await stream.CopyToAsync(fileStream);

            selectedImagePath = destination;

            ImageBox.Text = fileName;
        }

        // Validaciones para permitir solo números o números con decimal en los campos correspondientes

        private void OnlyNumbersDecimal_BeforeTextChanging(
            TextBox sender,
            TextBoxBeforeTextChangingEventArgs args
        )
        {
            string text = args.NewText;

            bool invalid =
                text.Any(c =>
                    !char.IsDigit(c) &&
                    c != '.')
                ||
                text.Count(c => c == '.') > 1;

            args.Cancel = invalid;
        }

        private void OnlyNumbers_BeforeTextChanging(
            TextBox sender,
            TextBoxBeforeTextChangingEventArgs args
        )
        {
            args.Cancel =
                args.NewText.Any(c => !char.IsDigit(c));
        }

        // Guarda los cambios del producto después de validar los campos y confirmar la contraseña de administrador

        private async void Save_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            var products =
                await JsonService.LoadAsync<Product>(
                    "products.json"
                );

            // VALIDACIONES

            if (!ValidateFields(
                products,
                out double costo,
                out double precio,
                out int existencias))
            {
                return;
            }

            int index = products.FindIndex(
                p => p.Id == product.Id
            );

            if (index < 0)
            {
                await ShowError(
                    "Producto no encontrado"
                );

                return;
            }

            if (!await ConfirmAdminPasswordAsync())
                return;

            int existenciasIniciales =
                product.Existencias;

            int diferencia =
                existencias - existenciasIniciales;

            // ACTUALIZAR PRODUCTO

            product.Nombre = NameBox.Text.Trim();
            product.Marca = BrandBox.Text.Trim();
            product.Descripcion = DescBox.Text.Trim();
            product.Costo = costo;
            product.PrecioVenta = precio;
            product.Existencias = existencias;
            product.Imagen = selectedImagePath;

            products[index] = product;

            await JsonService.SaveAsync(
                "products.json",
                products
            );

            // GUARDAR LOG

            if (diferencia != 0)
            {
                await SaveRestockLog(
                    existenciasIniciales,
                    diferencia,
                    existencias
                );
            }

            Close();
        }

        // Valida los campos del formulario y verifica que no haya otro producto con el mismo nombre y marca (ignorando mayúsculas)

        private bool ValidateFields(
            System.Collections.Generic.List<Product> products,
            out double costo,
            out double precio,
            out int existencias
        )
        {
            costo = 0;
            precio = 0;
            existencias = 0;

            if (
                string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(BrandBox.Text) ||
                string.IsNullOrWhiteSpace(CostBox.Text) ||
                string.IsNullOrWhiteSpace(PriceBox.Text) ||
                string.IsNullOrWhiteSpace(StockBox.Text)
            )
            {
                _ = ShowError(
                    "Todos los campos son obligatorios"
                );

                return false;
            }

            if (NameBox.Text.Trim().Length < 5)
            {
                _ = ShowError(
                    "El nombre debe tener al menos 5 caracteres"
                );

                return false;
            }

            if (
                !double.TryParse(CostBox.Text, out costo) ||
                !double.TryParse(PriceBox.Text, out precio) ||
                !int.TryParse(StockBox.Text, out existencias)
            )
            {
                _ = ShowError(
                    "Valores numéricos inválidos"
                );

                return false;
            }

            if (
                costo < 1 ||
                precio <= costo ||
                existencias < 0
            )
            {
                _ = ShowError(
                    "Valores inválidos"
                );

                return false;
            }

            bool exists = products.Exists(p =>
                p.Id != product.Id &&
                p.Nombre.Equals(
                    NameBox.Text.Trim(),
                    StringComparison.OrdinalIgnoreCase
                ) &&
                p.Marca.Equals(
                    BrandBox.Text.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (exists)
            {
                _ = ShowError(
                    "Ya existe un producto igual"
                );

                return false;
            }

            return true;
        }

        // Guarda un log de reabastecimiento o ajuste de existencias si hubo un cambio en las existencias del producto

        private async Task SaveRestockLog(
            int inicial,
            int diferencia,
            int final
        )
        {
            var logs =
                await JsonService.LoadAsync<RestockLog>(
                    "restockLogs.json"
                );

            logs.Add(new RestockLog
            {
                Id = IdGenerator.GetNextId(logs),

                Producto = product.Nombre,

                ExistenciasIniciales = inicial,

                ExistenciasAgregadas = diferencia,

                ExistenciasFinales = final,

                FechaModificacion = DateTime.Now,

                TipoMovimiento =
                    diferencia > 0
                        ? "Reabastecimiento"
                        : "Ajuste"
            });

            await JsonService.SaveAsync(
                "restockLogs.json",
                logs
            );
        }



        private async Task ShowError(
            string message
        )
        {
            await new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = Content.XamlRoot
            }.ShowAsync();
        }

        // Muestra un diálogo para confirmar la contraseña de administrador antes de permitir guardar los cambios en el producto

        private async Task<bool>
            ConfirmAdminPasswordAsync()
        {
            var passwordBox =
                 new PasswordBox
                 {
                     MaxLength = 10,
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

            var result =
                await new ContentDialog
                {
                    Title = "Confirmar administrador",

                    Content = passwordBox,

                    PrimaryButtonText = "Confirmar",

                    CloseButtonText = "Cancelar",

                    XamlRoot = Content.XamlRoot
                }.ShowAsync();

            if (
                result !=
                ContentDialogResult.Primary
            )
            {
                return false;
            }

            string password =
                passwordBox.Password;

            if (
                string.IsNullOrWhiteSpace(password) ||
                !ValidationHelper.IsValidPassword(password)
            )
            {
                await ShowError(
                    "Contraseña incorrecta"
                );

                return false;
            }

            var users =
                await JsonService.LoadAsync<User>(
                    "users.json"
                );

            return users.Any(u =>
                u.Rol == "Admin" &&
                u.Password == password
            );
        }
    }
}