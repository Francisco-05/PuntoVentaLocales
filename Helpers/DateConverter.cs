using Microsoft.UI.Xaml.Data;
using System;

namespace PuntoVenta.Helpers
{
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            //Convierte el valor a DateTime y lo formatea como "dd/MM/yyyy HH:mm"
            return ((DateTime)value).ToString("dd/MM/yyyy HH:mm");
        }
            
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}