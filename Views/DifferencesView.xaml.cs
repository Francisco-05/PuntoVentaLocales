using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PuntoVenta.Views
{
    public sealed partial class DifferencesView : Page
    {
        private List<DiferenciaCaja> allData = new();

        public DifferencesView()
        {
            this.InitializeComponent();

            // Fecha por defecto
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

        // Carga los datos desde el JSON y llena la lista y el filtro
        private async void LoadData()
        {
            var data = await JsonService.LoadAsync<DiferenciaCaja>("diferenciasCaja.json");

            if (data == null)
                data = new List<DiferenciaCaja>();

            allData = data;

            DifferencesList.ItemsSource = allData;

            // Lista de empleados únicos
            var empleados = allData
                .Select(d => d.Empleado)
                .Distinct()
                .ToList();

            empleados.Insert(0, "Todos");

            EmployeeFilter.ItemsSource = empleados;
            EmployeeFilter.SelectedIndex = 0;
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            var filtered = allData.AsEnumerable();

            // Filtro fecha inicio
            if (StartDate.Date != null)
            {
                var start = StartDate.Date.Value.Date;
                filtered = filtered.Where(d => d.Fecha >= start);
            }

            // Filtro fecha fin
            if (EndDate.Date != null)
            {
                var end = EndDate.Date.Value.Date.AddDays(1);
                filtered = filtered.Where(d => d.Fecha < end);
            }

            // Filtro empleado
            if (EmployeeFilter.SelectedItem != null &&
                EmployeeFilter.SelectedItem.ToString() != "Todos")
            {
                string emp = EmployeeFilter.SelectedItem.ToString();
                filtered = filtered.Where(d => d.Empleado == emp);
            }

            DifferencesList.ItemsSource = filtered.ToList();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.MainFrameControl.GoBack();
        }
    }
}