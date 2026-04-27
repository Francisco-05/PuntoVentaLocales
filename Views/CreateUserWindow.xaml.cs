using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Helpers;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PuntoVenta.Views
{
    public sealed partial class CreateUserWindow : Window
    {
        public CreateUserWindow()
        {
            this.InitializeComponent();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var users = await JsonService.LoadAsync<User>("users.json");

            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            // 🔥 CAMPOS VACÍOS
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneBox.Text) ||
                RoleBox.SelectedItem == null)
            {
                await ShowError("Todos los campos son obligatorios");
                return;
            }

            // 🔥 VALIDACIONES HELPERS
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

            // 🔥 DUPLICADO USERNAME
            if (users.Exists(u => u.Username.ToLower() == username.ToLower()))
            {
                await ShowError("El usuario ya existe");
                return;
            }

            var user = new User
            {
                Id = IdGenerator.GetNextId(users),
                Username = username,
                Password = password,
                NombreCompleto = NameBox.Text,
                Telefono = PhoneBox.Text,
                FechaNacimiento = BirthDatePicker.Date.DateTime,
                Rol = (RoleBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Empleado"
            };

            users.Add(user);

            await JsonService.SaveAsync("users.json", users);

            this.Close();
        }

        private async Task ShowError(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}