using System;
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

        //validacion de fecha de nacimiento no mayor a la fecha actual
        public static bool IsValidDateOfBirth(DateTime dateOfBirth)
        {
            DateTime today = DateTime.Now.Date;
            int age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //validacion de edad mayor a 18 años
        public static bool IsAdult(DateTime dateOfBirth)
        {
            DateTime today = DateTime.Now.Date;
            int age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }
            if (age >= 18)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}