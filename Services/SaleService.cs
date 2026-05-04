using PuntoVenta.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PuntoVenta.Services
{
    public static class SaleService
    {
        private const string FILE = "sales.json";

        // Obtener todas las ventas
        public static async Task<List<Sale>> GetAllAsync()
        {
            return await JsonService.LoadAsync<Sale>(FILE);
        }

        // Agregar venta
        public static async Task AddAsync(Sale sale)
        {
            var sales = await JsonService.LoadAsync<Sale>(FILE);

            sale.Id = sales.Count > 0 ? sales.Max(s => s.Id) + 1 : 1;

            sales.Add(sale);

            await JsonService.SaveAsync(FILE, sales);
        }
    }
}