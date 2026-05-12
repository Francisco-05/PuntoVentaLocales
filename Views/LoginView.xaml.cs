using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Helpers;
using PuntoVenta.Models;
using PuntoVenta.Services;
using PuntoVenta.Views;
using System;
using System.Linq;

namespace PuntoVenta.Views
{
    public sealed partial class LoginView : Page
    {
        public LoginView()
        {
            this.InitializeComponent();

            // Bloquear spam por tecla mantenida
            UsernameBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;
            PasswordBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;

            // Username - no permitir espacios
            UsernameBox.TextChanging += (s, e) =>
            {
                if (UsernameBox.Text.Contains(" "))
                {
                    int cursorPosition = UsernameBox.SelectionStart;

                    UsernameBox.Text = UsernameBox.Text.Replace(" ", "");

                    if (cursorPosition > 0)
                        UsernameBox.SelectionStart = cursorPosition - 1;
                }
            };

            // Password - no permitir espacios
            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (PasswordBox.Password.Contains(" "))
                {
                    PasswordBox.Password = PasswordBox.Password.Replace(" ", "");
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
    }
}