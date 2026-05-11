using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PuntoVenta.Views
{
    public sealed partial class RestockReportView : Page
    {
        private List<RestockLog> logs = new();

        public RestockReportView()
        {
            InitializeComponent();

            LoadLogs();
        }

        //CARGAR LOGS

        private async void LoadLogs()
        {
            logs = await JsonService.LoadAsync<RestockLog>(
                "restockLogs.json"
            );

            RestockList.ItemsSource =
                logs.OrderByDescending(
                    x => x.FechaModificacion
                );
        }



        //FILTRAR

        private void Filter_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            IEnumerable<RestockLog> filtered =
                logs;

            // FECHA DESDE

            if (StartDate.Date != null)
            {
                DateTime start =
                    StartDate.Date.Value.Date;

                filtered = filtered.Where(x =>
                    x.FechaModificacion.Date >= start
                );
            }

            // FECHA HASTA

            if (EndDate.Date != null)
            {
                DateTime end =
                    EndDate.Date.Value.Date;

                filtered = filtered.Where(x =>
                    x.FechaModificacion.Date <= end
                );
            }

            // TIPO MOVIMIENTO

            if (
                MovementFilter.SelectedItem
                is ComboBoxItem item
            )
            {
                string tipo =
                    item.Content.ToString();

                if (tipo != "Todos")
                {
                    filtered = filtered.Where(x =>
                        x.TipoMovimiento == tipo
                    );
                }
            }

            RestockList.ItemsSource =
                filtered.OrderByDescending(
                    x => x.FechaModificacion
                );
        }

        private void Back_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            MainWindow.Instance
                .MainFrameControl
                .Navigate(typeof(AdminView));
        }
    }
}