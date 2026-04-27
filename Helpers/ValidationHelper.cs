using System.Text.RegularExpressions;

namespace PuntoVenta.Helpers
{
    public static class ValidationHelper
    {
        // 🔥 Username mínimo 8 caracteres
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return username.Length >= 8;
        }

        // 🔥 Password segura:
        // min 8, 1 mayúscula, 1 minúscula, 1 número
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 8)
                return false;

            bool hasUpper = Regex.IsMatch(password, "[A-Z]");
            bool hasLower = Regex.IsMatch(password, "[a-z]");
            bool hasNumber = Regex.IsMatch(password, "[0-9]");

            return hasUpper && hasLower && hasNumber;
        }
    }
}