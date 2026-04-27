using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Services;
using PuntoVenta.Views;

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

            // 🔥 GUARDAR USUARIO EN SESIÓN
            SessionService.CurrentUser = user;

            // 👑 ADMIN
            if (user.Rol == "Admin")
            {
                MainWindow.Instance.MainFrameControl.Navigate(typeof(AdminView));
                return;
            }

            // 🧑 EMPLEADO → POS
            if (user.Rol == "Empleado")
            {
                MainWindow.Instance.MainFrameControl.Navigate(typeof(ProductCatalogView));
                return;
            }

            ErrorText.Text = "Rol no válido";
        }
    }
}