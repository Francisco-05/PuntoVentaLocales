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
            Init();
        }

        private async void Init()
        {
            await UserService.InitializeAsync();
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