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
            LoadData();
        }

        // Cargar datos de ventas
        private async void LoadData()
        {
            allSales = await SaleService.GetAllAsync();

            SalesList.ItemsSource = allSales;

            //cargar empleados únicos
            var empleados = allSales
                .Select(s => s.Empleado)
                .Distinct()
                .ToList();

            EmployeeFilter.ItemsSource = empleados;

            CalculateTotals(allSales);
        }

      
        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            var filtered = allSales.AsEnumerable();

            // Filtro por fecha
            if (StartDate.Date != default)
            {
                var start = StartDate.Date.DateTime.Date;

                filtered = filtered.Where(s =>
                    s.Fecha.Date >= start);
            }

            if (EndDate.Date != default)
            {
                var end = EndDate.Date.DateTime.Date;

                filtered = filtered.Where(s =>
                    s.Fecha.Date <= end);
            }

            // Filtro por empleado
            if (EmployeeFilter.SelectedItem != null)
            {
                string emp = EmployeeFilter.SelectedItem.ToString();
                filtered = filtered.Where(s => s.Empleado == emp);
            }

            var result = filtered.ToList();

            SalesList.ItemsSource = result;

            CalculateTotals(result);
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.Navigate(typeof(AdminView));
        }

        // Calcular totales de ventas y utilidad
        private void CalculateTotals(List<Sale> sales)
        {
            double totalVentas = sales.Sum(s => s.TotalBruto);
            double totalUtilidad = sales.Sum(s => s.Utilidad);

            TotalVentasText.Text = $"Ventas totales: {totalVentas:C2}";
            TotalUtilidadText.Text = $"Utilidad total: {totalUtilidad:C2}";
        }
    }
}