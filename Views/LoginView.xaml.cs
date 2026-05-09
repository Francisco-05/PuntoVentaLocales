using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Services;
using PuntoVenta.Views;
using System;

namespace PuntoVenta.Views
{
    public sealed partial class LoginView : Page
    {
        public LoginView()
        {
            this.InitializeComponent();

            // 🔥 BLOQUEAR ESPACIO COMO PRIMER CARACTER (USERNAME)
            UsernameBox.TextChanging += (s, e) =>
            {
                if (UsernameBox.Text.StartsWith(" "))
                {
                    UsernameBox.Text = UsernameBox.Text.TrimStart();
                    UsernameBox.SelectionStart = UsernameBox.Text.Length;
                }
            };

            // 🔥 BLOQUEAR ESPACIO COMO PRIMER CARACTER (PASSWORD)
            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (PasswordBox.Password.StartsWith(" "))
                {
                    PasswordBox.Password = PasswordBox.Password.TrimStart();
                }
            };

            Init();
        }

        private async void Init()
        {
            await UserService.InitializeAsync();
            SetupEnterNavigation();
        }

        private void SetupEnterNavigation()
        {
            UsernameBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    PasswordBox.Focus(FocusState.Programmatic);
                }
            };

            PasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    Login_Click(null, null);
                }
            };
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var user = await UserService.Login(
                UsernameBox.Text,
                PasswordBox.Password);

            if (user == null)
            {
                ErrorText.Text = "Usuario o contraseña incorrectos";
                return;
            }

            // Guardar usuario en sesión
            SessionService.CurrentUser = user;

            // Guardar hora de login
            SessionService.LoginTime = DateTime.Now;

            // ADMIN
            if (user.Rol == "Admin")
            {
                MainWindow.Instance.MainFrameControl.Navigate(typeof(AdminView));
                return;
            }

            // EMPLEADO
            if (user.Rol == "Empleado")
            {
                MainWindow.Instance.MainFrameControl.Navigate(typeof(ProductCatalogView));
                return;
            }

            ErrorText.Text = "Rol no válido";
        }

        private void AdminLogin_Click(object sender, RoutedEventArgs e)
        {
            SessionService.CurrentUser = null;
            SessionService.LoginTime = DateTime.Now;
            MainWindow.Instance.MainFrameControl.Navigate(typeof(AdminView));
        }

        private void EmployeeLogin_Click(object sender, RoutedEventArgs e)
        {
            SessionService.CurrentUser = null;
            SessionService.LoginTime = DateTime.Now;
            MainWindow.Instance.MainFrameControl.Navigate(typeof(ProductCatalogView));
        }
    }
}