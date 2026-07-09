using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PrimeraWebApp.Models;

namespace PrimeraWebApp.Controllers
{
    public class ProductoController : Controller
    {
        private readonly string _connectionString;

    
        public ProductoController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

 
        public IActionResult Index()
        {
            List<Producto> listaProductos = new List<Producto>();

            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
      
                string query = "SELECT p.id, p.nombre, p.precio, p.stock, c.nombre AS NombreCategoria " +
                               "FROM productos p " +
                               "INNER JOIN categoria c ON p.categoria = c.id_categoria";

                MySqlCommand cmd = new MySqlCommand(query, conexion);

                try
                {
                    conexion.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listaProductos.Add(new Producto
                            {
                                Id = reader.GetInt32("id"), 
                                Nombre = reader.GetString("nombre"),
                                Precio = reader.GetInt32("precio"),
                                Stock = reader.GetInt32("stock"),
                                NombreCategoria = reader.GetString("NombreCategoria")
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al listar productos: " + ex.Message;
                }
            }

            return View(listaProductos);
        }

    
        public IActionResult Create()
        {
            return View();
        }

       
        [HttpPost]
        public IActionResult Create(Producto producto)
        {
            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                try
                {
                    conexion.Open();

                
                    string queryProducto = "INSERT INTO productos (nombre, precio, stock, id_categoria) " +
                                           "VALUES (@nombre, @precio, @stock, @id_categoria); " +
                                           "SELECT LAST_INSERT_ID();";

                    MySqlCommand cmdProd = new MySqlCommand(queryProducto, conexion);
                    cmdProd.Parameters.AddWithValue("@nombre", producto.Nombre);
                    cmdProd.Parameters.AddWithValue("@precio", producto.Precio);
                    cmdProd.Parameters.AddWithValue("@stock", producto.Stock);
                    cmdProd.Parameters.AddWithValue("@categoria", producto.IdCategoria);

                   
                    int idProductoReciente = Convert.ToInt32(cmdProd.ExecuteScalar());


                 
                    string queryHistorial = "INSERT INTO historial_stock (id_producto, cantidad_movimiento, tipo_movimiento, fecha_hora) " +
                                            "VALUES (@id_producto, @cantidad, @tipo, @fecha_hora)";

                    MySqlCommand cmdHist = new MySqlCommand(queryHistorial, conexion);
                    cmdHist.Parameters.AddWithValue("@id_producto", idProductoReciente); 
                    cmdHist.Parameters.AddWithValue("@cantidad", producto.Stock);        
                    cmdHist.Parameters.AddWithValue("@tipo", "INGRESO INICIAL");
                    cmdHist.Parameters.AddWithValue("@fecha_hora", DateTime.Now);       

                    cmdHist.ExecuteNonQuery();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
               
                    ViewBag.Error = "Error en la transacción: " + ex.Message;
                    return View(producto);
                }
            }
        }
    }
}
