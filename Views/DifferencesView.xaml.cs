using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PuntoVenta.Views
{
    public sealed partial class DifferencesView : Page
    {
        private List<DiferenciaCaja> allData = new();

        public DifferencesView()
        {
            this.InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            var data = await JsonService.LoadAsync<DiferenciaCaja>("diferenciasCaja.json");

            if (data == null)
                data = new List<DiferenciaCaja>();

            allData = data;

            DifferencesList.ItemsSource = allData;

            // Obtener lista de empleados únicos para el filtro
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
            if (StartDate.Date != default(DateTimeOffset))
            {
                var start = StartDate.Date.Date;
                filtered = filtered.Where(d => d.Fecha >= start);
            }

            // Filtro fecha fin 
            if (EndDate.Date != default(DateTimeOffset))
            {
                var end = EndDate.Date.Date.AddDays(1);
                filtered = filtered.Where(d => d.Fecha < end);
            }

            // filtro empleado
            if (EmployeeFilter.SelectedItem != null &&
                EmployeeFilter.SelectedItem.ToString() != "Todos")
            {
                string emp = EmployeeFilter.SelectedItem.ToString(); // Obtener el nombre del empleado seleccionado
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