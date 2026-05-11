using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using Windows.System;

namespace PuntoVenta.Helpers
{
    public static class InputValidationHelper
    {
        private static readonly HashSet<VirtualKey> PressedKeys = new();
        //Evita que el usuario pueda ingresar espacios al inicio del texto en un TextBox
        public static void PreventLeadingSpaces(TextBox textBox)
        {
            if (textBox.Text.StartsWith(" "))
            {
                textBox.Text = textBox.Text.TrimStart();
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        // Evita que el usuario pueda ingresar espacios al inicio del texto en un PasswordBox

        public static void PreventHeldKeySpam(
            object sender,
            KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                return;

            // Si la tecla sigue presionada
            if (PressedKeys.Contains(e.Key))
            {
                e.Handled = true;
                return;
            }

            PressedKeys.Add(e.Key);

            if (sender is UIElement element)
            {
                element.KeyUp -= OnKeyUp;
                element.KeyUp += OnKeyUp;
            }
        }
        // Evita que el usuario pueda ingresar espacios al inicio del texto en un TextBox
        public static void PreventHeldKeySpamtextbox(
     object sender,
     KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                return;

            // La tecla ya estaba presionada
            if (e.KeyStatus.WasKeyDown)
            {
                e.Handled = true;
            }
        }

          
        private static void OnKeyUp(
            object sender,
            KeyRoutedEventArgs e)
        {
            PressedKeys.Remove(e.Key);
        }
        
 
        public static void ValidateTextBox(TextBox textBox)
        {
            textBox.TextChanging += (s, e) =>
            {
                PreventLeadingSpaces(textBox);
            };

            textBox.KeyDown += PreventHeldKeySpam;
        }

        public static void ReleaseKey(object sender, KeyRoutedEventArgs e)
        {
            PressedKeys.Remove(e.Key);
        }

        // Valida un PasswordBox para evitar espacios al inicio y prevenir spam de teclas mantenidas

        public static void ValidatePasswordBox(
            PasswordBox passwordBox)
        {
            passwordBox.PasswordChanged += (s, e) =>
            {
                if (passwordBox.Password.StartsWith(" "))
                {
                    passwordBox.Password =
                        passwordBox.Password.TrimStart();
                }
            };

            passwordBox.KeyDown += PreventHeldKeySpam;
        }
    }
}