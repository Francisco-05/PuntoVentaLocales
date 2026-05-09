using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Helpers;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PuntoVenta.Views
{
    public sealed partial class CreateUserWindow : Window
    {
        public CreateUserWindow()
        {
            this.InitializeComponent();
            SetupEnterNavigation();
        }

        private void SetupEnterNavigation()
        {
            UsernameBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    PasswordBox.Focus(FocusState.Programmatic);
                }
            };

            PasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    NameBox.Focus(FocusState.Programmatic);
                }
            };

            NameBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    PhoneBox.Focus(FocusState.Programmatic);
                }
            };

            PhoneBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    BirthDatePicker.Focus(FocusState.Programmatic);
                }
            };

            BirthDatePicker.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    RoleBox.Focus(FocusState.Programmatic);
                }
            };

            RoleBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;
                    Save_Click(null, null);
                }
            };
        }

        private void PhoneBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneBox.Text) ||
                RoleBox.SelectedItem == null)
            {
                await ShowError("Todos los campos son obligatorios");
                return;
            }

            if (PhoneBox.Text.Length != 10)
            {
                await ShowError("El número de teléfono debe tener 10 dígitos.");
                return;
            }

            if (!ValidationHelper.IsValidUsername(username))
            {
                await ShowError("Username inválido (mínimo 8 caracteres)");
                return;
            }

            if (!ValidationHelper.IsValidPassword(password))
            {
                await ShowError("Password débil (min 8, 1 mayúscula, 1 minúscula, 1 número)");
                return;
            }

            // Verifica que exista fecha seleccionada
            if (BirthDatePicker.Date == null)
            {
                await ShowError("Selecciona una fecha de nacimiento.");
                return;
            }

            DateTime birthDate = BirthDatePicker.Date.Value.DateTime;

            if (!ValidationHelper.IsValidDateOfBirth(birthDate))
            {
                await ShowError("La fecha no puede ser posterior a la actual.");
                return;
            }
            else if (!ValidationHelper.IsAdult(birthDate))
            {
                await ShowError("El usuario debe ser mayor de edad.");
                return;
            }

            var users = await JsonService.LoadAsync<User>("users.json");

            if (users.Exists(u => u.Username.ToLower() == username.ToLower()))
            {
                await ShowError("El usuario ya existe");
                return;
            }

            var user = new User
            {
                Username = username,
                Password = password,
                NombreCompleto = NameBox.Text,
                Telefono = PhoneBox.Text,
                FechaNacimiento = birthDate,
                Rol = (RoleBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Empleado"
            };

            await UserService.CreateUserAsync(user);

            await ShowError("Usuario guardado correctamente");

            this.Close();
        }

        private async Task ShowError(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Mensaje",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}