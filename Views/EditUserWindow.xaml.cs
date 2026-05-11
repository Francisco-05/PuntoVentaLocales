using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PuntoVenta.Helpers;
using PuntoVenta.Models;
using PuntoVenta.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PuntoVenta.Views
{
    public sealed partial class EditUserWindow : Window
    {
        private readonly User user;

        public EditUserWindow(User user)
        {
            InitializeComponent();

            this.user = user;

            LoadUser();

            // Bloquear spam por tecla mantenida
            UsernameBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;
            NameBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;
            PhoneBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;
            PasswordBox.PreviewKeyDown += InputValidationHelper.PreventHeldKeySpam;
            // Username
            UsernameBox.TextChanging += (s, e) =>
            {
                InputValidationHelper.PreventLeadingSpaces(UsernameBox);
            };

            // Password
            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (PasswordBox.Password.StartsWith(" "))
                {
                    PasswordBox.Password =
                        PasswordBox.Password.TrimStart();
                }
            };

            // Nombre
            NameBox.TextChanging += (s, e) =>
            {
                InputValidationHelper.PreventLeadingSpaces(NameBox);
            };

            // Teléfono
            PhoneBox.TextChanging += (s, e) =>
            {
                InputValidationHelper.PreventLeadingSpaces(PhoneBox);
            };

            SetupEnterNavigation();
        }

        // =========================================
        // CARGAR DATOS
        // =========================================

        private void LoadUser()
        {
            UsernameBox.Text =
                user.Username;

            PasswordBox.Password =
                user.Password;

            NameBox.Text =
                user.NombreCompleto;

            PhoneBox.Text =
                user.Telefono;
        }

        // =========================================
        // ENTER NAVIGATION
        // =========================================

        private void SetupEnterNavigation()
        {
            UsernameBox.KeyDown += (s, e) =>
            {
                if (e.Key ==
                    Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;

                    PasswordBox.Focus(
                        FocusState.Programmatic
                    );
                }
            };

            PasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key ==
                    Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;

                    NameBox.Focus(
                        FocusState.Programmatic
                    );
                }
            };

            NameBox.KeyDown += (s, e) =>
            {
                if (e.Key ==
                    Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;

                    PhoneBox.Focus(
                        FocusState.Programmatic
                    );
                }
            };

            PhoneBox.KeyDown += (s, e) =>
            {
                if (e.Key ==
                    Windows.System.VirtualKey.Enter)
                {
                    e.Handled = true;

                    Save_Click(null, null);
                }
            };
        }

        // =========================================
        // SOLO NÚMEROS TELÉFONO
        // =========================================

        private void PhoneBox_BeforeTextChanging(
            TextBox sender,
            TextBoxBeforeTextChangingEventArgs args
        )
        {
            args.Cancel =
                args.NewText.Any(c =>
                    !char.IsDigit(c)
                );
        }

        // =========================================
        // GUARDAR
        // =========================================

        private async void Save_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            string username =
                UsernameBox.Text;

            string password =
                PasswordBox.Password;

            // =========================================
            // CAMPOS VACÍOS
            // =========================================

            if (
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneBox.Text)
            )
            {
                await ShowMessage(
                    "Todos los campos son obligatorios"
                );

                return;
            }

            // =========================================
            // TELÉFONO
            // =========================================

            if (PhoneBox.Text.Length != 10)
            {
                await ShowMessage(
                    "El número de teléfono debe tener 10 dígitos."
                );

                return;
            }

            // =========================================
            // USERNAME
            // =========================================

            if (
                !ValidationHelper
                    .IsValidUsername(username)
            )
            {
                await ShowMessage(
                    "Username inválido (mínimo 8 caracteres)"
                );

                return;
            }

            // =========================================
            // PASSWORD
            // =========================================

            if (
                !ValidationHelper
                    .IsValidPassword(password)
            )
            {
                await ShowMessage(
                    "Password débil (min 8, 1 mayúscula, 1 minúscula, 1 número)"
                );

                return;
            }

            // =========================================
            // CARGAR USERS
            // =========================================

            var users =
                await JsonService.LoadAsync<User>(
                    "users.json"
                );

            // =========================================
            // VALIDAR USERNAME DUPLICADO
            // =========================================

            bool exists =
                users.Any(u =>
                    u.Username.ToLower() ==
                        username.ToLower() &&
                    u.Username != user.Username
                );

            if (exists)
            {
                await ShowMessage(
                    "El usuario ya existe"
                );

                return;
            }

            // =========================================
            // ACTUALIZAR
            // =========================================

            var index =
                users.FindIndex(u =>
                    u.Username ==
                    user.Username
                );

            if (index < 0)
            {
                return;
            }

            user.Username =
                username;

            user.Password =
                password;

            user.NombreCompleto =
                NameBox.Text;

            user.Telefono =
                PhoneBox.Text;

            users[index] = user;

            // =========================================
            // GUARDAR
            // =========================================

            await JsonService.SaveAsync(
                "users.json",
                users
            );

            await ShowMessage(
                "Usuario actualizado correctamente"
            );

            this.Close();
        }

        // =========================================
        // MENSAJES
        // =========================================

        private async Task ShowMessage(
            string message
        )
        {
            ContentDialog dialog =
                new ContentDialog
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