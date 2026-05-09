using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PuntoVenta.Views
{
    public sealed partial class ReportView : Page
    {
        private List<Sale> allSales;

        public ReportView()
        {
            this.InitializeComponent();

            // Fechas por defecto
            StartDate.Date = DateTimeOffset.Now.AddDays(-7);
            EndDate.Date = DateTimeOffset.Now;

            // Evita fechas futuras
            StartDate.MaxDate = DateTimeOffset.Now;
            EndDate.MaxDate = DateTimeOffset.Now;

            SetupNavigation();

            LoadData();
        }

        private void SetupNavigation()
        {
            StartDate.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    EndDate.Focus(FocusState.Programmatic);
                }
            };

            EndDate.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    EmployeeFilter.Focus(FocusState.Programmatic);
                }
            };

            EmployeeFilter.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    Filter_Click(null, null);
                }
            };
        }

        private async void LoadData()
        {
            allSales = await SaleService.GetAllAsync();

            SalesList.ItemsSource = allSales;

            // Empleados únicos
            var empleados = allSales
                .Select(s => s.Empleado)
                .Distinct()
                .ToList();

            empleados.Insert(0, "Todos");

            EmployeeFilter.ItemsSource = empleados;
            EmployeeFilter.SelectedIndex = 0;

            CalculateTotals(allSales);
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            var filtered = allSales.AsEnumerable();

            // Filtro por fecha inicial
            if (StartDate.Date != null)
            {
                var start = StartDate.Date.Value.Date;

                filtered = filtered.Where(s =>
                    s.Fecha.Date >= start);
            }

            // Filtro por fecha final
            if (EndDate.Date != null)
            {
                var end = EndDate.Date.Value.Date.AddDays(1);

                filtered = filtered.Where(s =>
                    s.Fecha < end);
            }

            // Filtro por empleado
            if (EmployeeFilter.SelectedItem != null &&
                EmployeeFilter.SelectedItem.ToString() != "Todos")
            {
                string emp = EmployeeFilter.SelectedItem.ToString();

                filtered = filtered.Where(s =>
                    s.Empleado == emp);
            }

            var result = filtered.ToList();

            SalesList.ItemsSource = result;

            CalculateTotals(result);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(AdminView));
        }

        // Calcula totales
        private void CalculateTotals(List<Sale> sales)
        {
            double totalVentas = sales.Sum(s => s.TotalBruto);
            double totalUtilidad = sales.Sum(s => s.Utilidad);

            TotalVentasText.Text = $"Ventas totales: {totalVentas:C2}";
            TotalUtilidadText.Text = $"Utilidad total: {totalUtilidad:C2}";
        }
    }
}