using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Views;
using System;
using WinRT.Interop;

namespace PuntoVenta
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance;

        public Frame MainFrameControl =>
            MainFrame;

        public MainWindow()
        {
            this.InitializeComponent();

            Instance = this;

            // MAXIMIZAR
            MaximizeWindow();

            // LOGIN
            MainFrame.Navigate(
                typeof(LoginView)
            );
        }

        private void MaximizeWindow()
        {
            IntPtr hWnd =
                WindowNative.GetWindowHandle(this);

            WindowId windowId =
                Win32Interop.GetWindowIdFromWindow(hWnd);

            AppWindow appWindow =
                AppWindow.GetFromWindowId(windowId);

            if (
                appWindow.Presenter
                    is OverlappedPresenter presenter
            )
            {
                presenter.Maximize();
            }
        }
    }
}
