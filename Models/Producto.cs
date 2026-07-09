namespace PrimeraWebApp.Models
{
    public class Producto
    {
 
        public int Id { get; set; }


        public string Nombre { get; set; }
        public int Precio { get; set; }
        public int Stock { get; set; }

     
        public int IdCategoria { get; set; }

        public string NombreCategoria { get; set; }
    }
}
