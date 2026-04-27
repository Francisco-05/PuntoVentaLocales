using Microsoft.UI.Xaml.Data;
using System;

namespace PuntoVenta.Helpers
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double precio)
            {
                return precio.ToString("C2");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}