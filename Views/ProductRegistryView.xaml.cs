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
        // =========================================
        // LISTAS
        // =========================================

        private List<Product> products = new();

        private List<User> users = new();

        public ProductRegistryView()
        {
            this.InitializeComponent();

            LoadProducts();

            LoadUsers();
        }



        // =========================================
        // CARGAR PRODUCTOS
        // =========================================

        private async void LoadProducts()
        {
            products =
                await JsonService.LoadAsync<Product>(
                    "products.json"
                );

            ProductsList.ItemsSource = products;
        }



        // =========================================
        // CARGAR USUARIOS
        // =========================================

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

        // =========================================
        // CAMBIAR A PRODUCTOS
        // =========================================

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

        // =========================================
        // CAMBIAR A USUARIOS
        // =========================================

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

        // =========================================
        // MODIFICAR PRODUCTO
        // =========================================

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

        // =========================================
        // REABASTECIMIENTO
        // =========================================

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

        // =========================================
        // ELIMINAR PRODUCTO
        // =========================================

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

        // =========================================
        // MODIFICAR USUARIO
        // =========================================

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
        }

        // =========================================
        // ELIMINAR USUARIO
        // =========================================

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

        // =========================================
        // REFRESCAR TABLAS
        // =========================================

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

        // =========================================
        // SOLO NÚMEROS
        // =========================================

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

        // =========================================
        // GUARDAR LOG
        // =========================================

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

        // =========================================
        // CONFIRMAR ADMIN
        // =========================================

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

        // =========================================
        // VOLVER
        // =========================================

        private void Back_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            MainWindow.Instance
                .MainFrameControl
                .Navigate(typeof(AdminView));
        }

        // =========================================
        // ERROR
        // =========================================

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