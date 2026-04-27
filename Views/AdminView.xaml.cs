using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Views;

namespace PuntoVenta.Views
{
    public sealed partial class AdminView : Page
    {
        public AdminView()
        {
            this.InitializeComponent();
        }

        // 🧑 Crear usuario
        private void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            var win = new CreateUserWindow();
            win.Activate();
        }

        // 📦 Crear producto
        private void CreateProduct_Click(object sender, RoutedEventArgs e)
        {
            var win = new CreateProductWindow();
            win.Activate();
        }

        // 📊 Reportes (lo dejamos listo para después)
        private void ReportView_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(ReportView));
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(LoginView));
        }
    }
}