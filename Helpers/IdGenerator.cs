using System.Collections.Generic;
using System.Linq;

namespace PuntoVenta.Helpers
{
    public static class IdGenerator
    {
        public static int GetNextId<T>(List<T> list)
        {
            // Si la lista es nula o está vacía, el próximo ID será 1
            if (list == null || list.Count == 0)
                return 1;
            
            var prop = typeof(T).GetProperty("Id");
            
            if (prop == null)
                return 1;
            // Obtener el valor máximo de la propiedad "Id" en la lista y sumarle 1
            int maxId = list
                .Select(x => (int)prop.GetValue(x))
                .DefaultIfEmpty(0)
                .Max();

            return maxId + 1;
        }
    }
}