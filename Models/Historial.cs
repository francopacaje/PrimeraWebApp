namespace PrimeraWebApp.Models
{
    public class Historial
    {
        public int IdHistorial { get; set; }
        public string NombreProducto { get; set; }
        public int CantidadMovimiento { get; set; }
        public string TipoMovimiento { get; set; }
        public DateTime FechaHora { get; set; }
    }
}
