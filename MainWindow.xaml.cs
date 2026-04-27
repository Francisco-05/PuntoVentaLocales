using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Views;


namespace PuntoVenta
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance;

        public Frame MainFrameControl => MainFrame;

        public MainWindow()
        {
            this.InitializeComponent();
            Instance = this;

            MainFrame.Navigate(typeof(LoginView));
        }
    }
}