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

            // Username - no permitir espacios
            UsernameBox.TextChanging += (s, e) =>
            {
                if (UsernameBox.Text.Contains(" "))
                {
                    int cursorPosition = UsernameBox.SelectionStart;

                    UsernameBox.Text = UsernameBox.Text.Replace(" ", "");

                    if (cursorPosition > 0)
                        UsernameBox.SelectionStart = Math.Min(cursorPosition - 1, UsernameBox.Text.Length);
                }
            };

            // Password - no permitir espacios
            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (PasswordBox.Password.Contains(" "))
                {
                    PasswordBox.Password = PasswordBox.Password.Replace(" ", "");
                }
            };

            // Nombre completo - no permitir espacios al inicio ni dobles espacios
            NameBox.TextChanging += (s, e) =>
            {
                CleanTextBoxSpaces(NameBox);
            };

            // Teléfono
            PhoneBox.TextChanging += (s, e) =>
            {
                InputValidationHelper.PreventLeadingSpaces(PhoneBox);
            };



            SetupEnterNavigation();


        }

        //Carga los datos del usuario en los campos correspondientes

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

        // Configura la navegación con Enter entre los campos del formulario

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


        // Limpia los espacios al inicio y dobles espacios en un TextBox, manteniendo la posición del cursor
        private void CleanTextBoxSpaces(TextBox box)
        {
            int cursorPosition = box.SelectionStart;

            string nuevoTexto = box.Text.TrimStart();

            while (nuevoTexto.Contains("  "))
            {
                nuevoTexto = nuevoTexto.Replace("  ", " ");
            }

            if (box.Text != nuevoTexto)
            {
                box.Text = nuevoTexto;

                if (cursorPosition > 0)
                {
                    box.SelectionStart = Math.Min(cursorPosition - 1, box.Text.Length);
                }
            }
        }


        // Valida que solo se puedan ingresar dígitos en el campo de teléfono

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

        // Maneja el clic en el botón de guardar, validando los campos y actualizando el usuario

        private async void Save_Click(
                object sender,
                RoutedEventArgs e)
        {
            bool confirmed =
                await ConfirmAdminPasswordAsync();

            if (!confirmed)
            {
                await ShowMessage(
                    "Se requiere confirmación de administrador"
                );

                return;
            }

            string username =
                UsernameBox.Text.Trim();

            string password =
                PasswordBox.Password.Trim();

            //Validar campos vacíos
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

 
            if (PhoneBox.Text.Length != 10)
            {
                await ShowMessage(
                    "El número de teléfono debe tener 10 dígitos."
                );

                return;
            }

            

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



            var users =
                await JsonService.LoadAsync<User>(
                    "users.json"
                );


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

            // Encontrar el índice del usuario que se está editando

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
                NameBox.Text.Trim();

            user.Telefono =
                PhoneBox.Text.Trim();

            users[index] = user;


            // Guardar los cambios en el archivo JSON

            await JsonService.SaveAsync(
                "users.json",
                users
            );

            await ShowMessage(
                "Usuario actualizado correctamente"
            );

            this.Close();
        }

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

        private async Task<bool>
        ConfirmAdminPasswordAsync()
        {
            var passwordBox =
                 new PasswordBox
                 {
                     MaxLength = 10,
                     Padding = new Thickness(40, 6, 0, 0)
                 };


            
            passwordBox.PasswordChanged += (s, e) =>
            {
                if (passwordBox.Password.Contains(" "))
                {
                    passwordBox.Password =
                        passwordBox.Password.Replace(" ", "");
                }
            };

            var result =
                await new ContentDialog
                {
                    Title = "Confirmar administrador",

                    Content = passwordBox,

                    PrimaryButtonText = "Confirmar",

                    CloseButtonText = "Cancelar",

                    XamlRoot = this.Content.XamlRoot
                }.ShowAsync();

            if (
                result !=
                ContentDialogResult.Primary
            )
            {
                return false;
            }

            string password =
                passwordBox.Password;

            if (
                string.IsNullOrWhiteSpace(password) ||
                !ValidationHelper.IsValidPassword(password)
            )
            {
                await ShowMessage(
                    "Contraseña incorrecta"
                );

                return false;
            }

            var users =
                await JsonService.LoadAsync<User>(
                    "users.json"
                );

            return users.Any(u =>
                u.Rol == "Admin" &&
                u.Password == password
            );
        }
    }
}