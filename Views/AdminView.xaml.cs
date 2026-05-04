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

        // 📊 Reporte de ventas
        private void ReportView_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(ReportView));
        }

        // 🧾 🔥 NUEVO → REPORTE DIFERENCIAS
        private void DifView_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(DifferencesView));
        }

        // 🚪 Logout
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(LoginView));
        }
    }
}