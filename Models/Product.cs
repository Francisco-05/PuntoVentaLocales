namespace PuntoVenta.Models
{

    public class Product
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Marca { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public double Costo { get; set; }
        public double PrecioVenta { get; set; }
        public string Imagen { get; set; } = "";
    }
}