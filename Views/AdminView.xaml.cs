using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace PuntoVenta.Views
{
    public sealed partial class AdminView : Page
    {
        public AdminView()
        {
            this.InitializeComponent();
        }

        // Crear usuario
        private void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            var win = new CreateUserWindow();
            win.Activate();
        }

        // Crear producto
        private void CreateProduct_Click(object sender, RoutedEventArgs e)
        {
            var win = new CreateProductWindow();
            win.Activate();
        }

        // Reporte de ventas
        private void ReportView_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(ReportView));
        }

        // Reporte de diferencias
        private void DifView_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(DifferencesView));
        }

        private void ProductRegistry_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(ProductRegistryView));
        }

        // Función para cerrar sesión
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(LoginView));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AnimarBoton(BtnCrearEmpleado, 0);
            AnimarBoton(BtnCrearProducto, 150);
            AnimarBoton(BtnReportes, 300);
            AnimarBoton(BtnDiferencias, 450);
            AnimarBoton(BtnRegistroProductos, 600);
            AnimarBoton(BtnCerrarSesion, 750);
        }

        private void AnimarBoton(Button boton, int delay)
        {
            Storyboard storyboard = new Storyboard();

            DoubleAnimation animacionMovimiento = new DoubleAnimation
            {
                From = 60,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                BeginTime = TimeSpan.FromMilliseconds(delay)
            };

            DoubleAnimation animacionOpacidad = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                BeginTime = TimeSpan.FromMilliseconds(delay)
            };

            Storyboard.SetTarget(animacionMovimiento, boton);
            Storyboard.SetTargetProperty(animacionMovimiento, "(UIElement.RenderTransform).(TranslateTransform.Y)");

            Storyboard.SetTarget(animacionOpacidad, boton);
            Storyboard.SetTargetProperty(animacionOpacidad, "Opacity");

            storyboard.Children.Add(animacionMovimiento);
            storyboard.Children.Add(animacionOpacidad);

            storyboard.Begin();
        }
    }
}