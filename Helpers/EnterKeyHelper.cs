using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PuntoVenta.Helpers
{
    /// <summary>
    /// Helper para manejar navegación con Enter en formularios.
    /// Permite avanzar al siguiente contenedor con Enter y ejecutar un botón si es el último.
    /// </summary>
    public static class EnterKeyHelper
    {
        /// <summary>
        /// Registra la navegación con Enter para los contenedores especificados.
        /// </summary>
        /// <param name="containers">Lista de contenedores en orden (TextBox, PasswordBox, ComboBox, DatePicker, etc.)</param>
        /// <param name="submitButton">Botón a ejecutar cuando se presiona Enter en el último contenedor</param>
        public static void SetupEnterNavigation(List<Control> containers, Button submitButton = null)
        {
            if (containers == null || containers.Count == 0)
                return;

            for (int i = 0; i < containers.Count; i++)
            {
                var container = containers[i];
                int currentIndex = i;

                if (container is TextBox textBox)
                {
                    textBox.KeyDown += (s, e) => HandleKeyDown(e, containers, currentIndex, submitButton);
                }
                else if (container is PasswordBox passwordBox)
                {
                    passwordBox.KeyDown += (s, e) => HandleKeyDown(e, containers, currentIndex, submitButton);
                }
                else if (container is ComboBox comboBox)
                {
                    comboBox.KeyDown += (s, e) => HandleKeyDown(e, containers, currentIndex, submitButton);
                }
                else if (container is DatePicker datePicker)
                {
                    datePicker.KeyDown += (s, e) => HandleKeyDown(e, containers, currentIndex, submitButton);
                }
            }
        }

        private static void HandleKeyDown(KeyRoutedEventArgs e, List<Control> containers, int currentIndex, Button submitButton)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;

            e.Handled = true;

            // Si es el último contenedor
            if (currentIndex == containers.Count - 1)
            {
                // Ejecutar botón si existe
                if (submitButton != null)
                {
                    submitButton.Focus(FocusState.Keyboard);
                }
            }
            else
            {
                // Avanzar al siguiente contenedor
                var nextContainer = containers[currentIndex + 1];
                nextContainer.Focus(FocusState.Programmatic);
            }
        }
    }
}
