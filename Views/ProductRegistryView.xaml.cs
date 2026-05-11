using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Helpers;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace PuntoVenta.Views
{
    public sealed partial class ProductRegistryView : Page
    {
        // Listas para almacenar productos y usuarios cargados desde JSON

        private List<Product> products = new();

        private List<User> users = new();

        public ProductRegistryView()
        {
            this.InitializeComponent();

            LoadProducts();

            LoadUsers();
        }



        //Cargar productos desde JSON y asignarlos al ListView

        private async void LoadProducts()
        {
            products =
                await JsonService.LoadAsync<Product>(
                    "products.json"
                );

            ProductsList.ItemsSource = products;
        }




        //Cargar usuarios desde JSON y asignarlos al ListView

        private async void LoadUsers()
        {
            users =
                await JsonService.LoadAsync<User>(
                    "users.json"
                );

            UsersList.ItemsSource = users;
        }

        private void Refresh_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadProducts();
            LoadUsers();


        }

        //Cambiar a productos

        private void ProductsButton_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            ProductsSection.Visibility =
                Visibility.Visible;

            UsersSection.Visibility =
                Visibility.Collapsed;

            TitleText.Text =
                "Registro de productos";

            SubtitleText.Text =
                "Consulta y administra productos";
        }

        // Cambiar a usuarios
        private void UsersButton_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            ProductsSection.Visibility =
                Visibility.Collapsed;

            UsersSection.Visibility =
                Visibility.Visible;

            TitleText.Text =
                "Registro de usuarios";

            SubtitleText.Text =
                "Consulta y administra usuarios";
        }


        //Modificar producto

        private void Modify_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            if (
                sender is Button button &&
                button.DataContext is Product product
            )
            {
                var win =
                    new EditProductWindow(product);

                win.Activate();
            }
        }


        //Metodo para reabastecer producto

        private async void Restock_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            if (
                sender is not Button button ||
                button.DataContext is not Product product
            )
            {
                return;
            }

            var amountBox = new TextBox
            {
                PlaceholderText = "Cantidad a añadir",
                MaxLength = 4

            };
            amountBox.BeforeTextChanging +=
            OnlyNumbers_BeforeTextChanging;
            var dialog = new ContentDialog
            {
                Title = "Reabastecimiento",

                Content = amountBox,

                PrimaryButtonText = "Agregar",

                CloseButtonText = "Cancelar",

                XamlRoot = this.XamlRoot
            };


            var result =
                await dialog.ShowAsync();

            if (
                result !=
                ContentDialogResult.Primary
            )
            {
                return;
            }

            if (
                !int.TryParse(
                    amountBox.Text,
                    out int amount
                ) ||
                amount <= 0
            )
            {
                await ShowError(
                    "Ingresa una cantidad válida mayor a 0"
                );

                return;
            }

            if (
                !await ConfirmAdminPasswordAsync()
            )
            {
                return;
            }

            int existenciasIniciales =
                product.Existencias;

            product.Existencias += amount;

            int existenciasFinales =
                product.Existencias;

            await JsonService.SaveAsync(
                "products.json",
                products
            );

            await SaveRestockLog(
                product,
                existenciasIniciales,
                amount,
                existenciasFinales
            );

            RefreshProductsTable();
        }

        //Eliminar producto

        private async void Delete_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            if (
                sender is not Button button ||
                button.DataContext is not Product product
            )
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Eliminar producto",

                Content =
                    $"¿Deseas eliminar el producto {product.Nombre}?",

                PrimaryButtonText = "Eliminar",

                CloseButtonText = "Cancelar",

                XamlRoot = this.XamlRoot
            };

            var result =
                await dialog.ShowAsync();

            if (
                result !=
                ContentDialogResult.Primary
            )
            {
                return;
            }

            if (
                !await ConfirmAdminPasswordAsync()
            )
            {
                return;
            }

            products.Remove(product);

            await JsonService.SaveAsync(
                "products.json",
                products
            );

            RefreshProductsTable();
        }

        //Modificar usuario

        private void ModifyUser_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            if (
                sender is Button button &&
                button.DataContext is User user
            )
            {
                var win =
                    new EditUserWindow(user);

                win.Activate();
            }

            //Eliminar usuario

        private async void DeleteUser_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            if (
                sender is not Button button ||
                button.DataContext is not User user
            )
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Eliminar usuario",

                Content =
                    $"¿Deseas eliminar el usuario {user.Username}?",

                PrimaryButtonText = "Eliminar",

                CloseButtonText = "Cancelar",

                XamlRoot = this.XamlRoot
            };

            var result =
                await dialog.ShowAsync();

            if (
                result !=
                ContentDialogResult.Primary
            )
            {
                return;
            }

            if (
                !await ConfirmAdminPasswordAsync()
            )
            {
                return;
            }

            users.Remove(user);

            await JsonService.SaveAsync(
                "users.json",
                users
            );

            RefreshUsersTable();
        }

        //Refrescar tablas

        private void RefreshProductsTable()
        {
            ProductsList.ItemsSource = null;

            ProductsList.ItemsSource = products;
        }

        private void RefreshUsersTable()
        {
            UsersList.ItemsSource = null;

            UsersList.ItemsSource = users;
        }

        // Validar que solo se puedan ingresar dígitos en el campo de cantidad
        private void OnlyNumbers_BeforeTextChanging(
            TextBox sender,
            TextBoxBeforeTextChangingEventArgs args
        )
        {
            string text = args.NewText;

            if (
                text.Any(c =>
                    !char.IsDigit(c)
                )
            )
            {
                args.Cancel = true;
            }
        }

        // Guardar un registro de reabastecimiento en un archivo JSON

        private async Task SaveRestockLog(
            Product product,
            int existenciasIniciales,
            int cantidadAgregada,
            int existenciasFinales
        )
        {
            var restockLogs =
                await JsonService.LoadAsync<RestockLog>(
                    "restockLogs.json"
                );

            restockLogs.Add(new RestockLog
            {
                Id = Guid.NewGuid(),

                Producto = product.Nombre,

                ExistenciasIniciales =
                    existenciasIniciales,

                ExistenciasAgregadas =
                    cantidadAgregada,

                ExistenciasFinales =
                    existenciasFinales,

                FechaModificacion =
                    DateTime.Now,

                TipoMovimiento =
                    "Reabastecimiento"
            });

            await JsonService.SaveAsync(
                "restockLogs.json",
                restockLogs
            );
        }

        //Confirmar contraseña de administrador para acciones sensibles

        private async Task<bool>
            ConfirmAdminPasswordAsync()
        {
            var passwordBox =
                 new PasswordBox
                 {
                     MaxLength = 12,
                     Padding= new Thickness(40, 6, 0, 0)
                 };

            passwordBox.PasswordChanged += (s, e) =>
            {
                if (passwordBox.Password.Contains(" "))
                {
                    passwordBox.Password =
                        passwordBox.Password.Replace(" ", "");
                }
            };

            var dialog =
                new ContentDialog
                {
                    Title =
                        "Confirmar administrador",

                    Content =
                        passwordBox,

                    PrimaryButtonText =
                        "Confirmar",

                    CloseButtonText =
                        "Cancelar",

                    XamlRoot =
                        this.XamlRoot
                };

            var result =
                await dialog.ShowAsync();

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
                string.IsNullOrWhiteSpace(
                    password
                )
            )
            {
                await ShowError(
                    "Todos los campos son obligatorios"
                );

                return false;
            }

            if (
                !ValidationHelper
                    .IsValidPassword(password)
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

            bool isAdmin = users.Any(u =>
                u.Rol == "Admin" &&
                u.Password == password
            );

            if (!isAdmin)
            {
                await ShowError(
                    "Contraseña incorrecta"
                );

                return false;
            }

            return true;
        }



        private void Back_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            MainWindow.Instance
                .MainFrameControl
                .Navigate(typeof(AdminView));
        }



        private async Task ShowError(
            string message
        )
        {
            var dialog =
                new ContentDialog
                {
                    Title = "Error",

                    Content = message,

                    CloseButtonText = "OK",

                    XamlRoot = this.XamlRoot
                };

            await dialog.ShowAsync();
        }
    }
}