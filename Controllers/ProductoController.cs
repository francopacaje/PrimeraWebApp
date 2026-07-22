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

        // 1. LISTAR PRODUCTOS (Muestra la tabla)
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
                                Precio = Convert.ToInt32(reader["precio"]),
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

        // Vista para cargar el formulario de creación
        public IActionResult Create()
        {
            return View();
        }

        // 2. INSERTAR PRODUCTO E HISTORIAL
        [HttpPost]
        public IActionResult Create(Producto producto)
        {
            if (producto.Stock < 0)
            {
                ViewBag.Error = "El stock inicial no puede ser un número negativo.";
                return View(producto);
            }

            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                try
                {
                    conexion.Open();

                    string queryProducto = "INSERT INTO productos (nombre, precio, stock, categoria) " +
                                           "VALUES (@nombre, @precio, @stock, @categoria); " +
                                           "SELECT LAST_INSERT_ID();";

                    MySqlCommand cmdProd = new MySqlCommand(queryProducto, conexion);
                    cmdProd.Parameters.AddWithValue("@nombre", producto.Nombre);
                    cmdProd.Parameters.AddWithValue("@precio", producto.Precio);
                    cmdProd.Parameters.AddWithValue("@stock", producto.Stock);
                    cmdProd.Parameters.AddWithValue("@categoria", producto.IdCategoria);

                    int idProductoReciente = Convert.ToInt32(cmdProd.ExecuteScalar());

                    string queryHistorial = "INSERT INTO historial (id_producto, cantidad_movimiento, tipo_movimiento, fecha_hora) " +
                                            "VALUES (@id_producto, @cantidad, @tipo, NOW())";

                    MySqlCommand cmdHist = new MySqlCommand(queryHistorial, conexion);
                    cmdHist.Parameters.AddWithValue("@id_producto", idProductoReciente);
                    cmdHist.Parameters.AddWithValue("@cantidad", producto.Stock);
                    cmdHist.Parameters.AddWithValue("@tipo", "INGRESO INICIAL");

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

        // 3. MOSTRAR FORMULARIO DE EDICIÓN (GET)
        [HttpGet]
        public IActionResult Editar(int id)
        {
            Producto producto = null;

            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                string query = "SELECT id, nombre, precio, stock, categoria FROM productos WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@id", id);

                try
                {
                    conexion.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            producto = new Producto
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Nombre = reader["nombre"].ToString(),
                                Precio = Convert.ToInt32(reader["precio"]),
                                Stock = Convert.ToInt32(reader["stock"]),
                                IdCategoria = Convert.ToInt32(reader["categoria"])
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al obtener el producto: " + ex.Message;
                }
            }

            if (producto == null)
            {
                return RedirectToAction("Index");
            }

            return View(producto);
        }

        // 4. GUARDAR EDICIÓN (UPDATE) + REGISTRO EN HISTORIAL (POST)
        [HttpPost]
        public IActionResult Editar(Producto producto)
        {
            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                conexion.Open();
                MySqlTransaction transaccion = conexion.BeginTransaction();

                try
                {
                    // Update del producto
                    string queryUpdate = "UPDATE productos SET nombre = @nombre, precio = @precio, stock = @stock, categoria = @categoria WHERE id = @id";
                    MySqlCommand cmdUpdate = new MySqlCommand(queryUpdate, conexion, transaccion);
                    cmdUpdate.Parameters.AddWithValue("@nombre", producto.Nombre);
                    cmdUpdate.Parameters.AddWithValue("@precio", producto.Precio);
                    cmdUpdate.Parameters.AddWithValue("@stock", producto.Stock);
                    cmdUpdate.Parameters.AddWithValue("@categoria", producto.IdCategoria);
                    cmdUpdate.Parameters.AddWithValue("@id", producto.Id);

                    cmdUpdate.ExecuteNonQuery();

                    // Insert en el historial para auditoría
                    string queryHistorial = "INSERT INTO historial (id_producto, cantidad_movimiento, tipo_movimiento, fecha_hora) " +
                                            "VALUES (@id_producto, @cantidad, @tipo, NOW())";
                    MySqlCommand cmdHistorial = new MySqlCommand(queryHistorial, conexion, transaccion);
                    cmdHistorial.Parameters.AddWithValue("@id_producto", producto.Id);
                    cmdHistorial.Parameters.AddWithValue("@cantidad", producto.Stock);
                    cmdHistorial.Parameters.AddWithValue("@tipo", "MODIFICACIÓN DE PRODUCTO");

                    cmdHistorial.ExecuteNonQuery();

                    transaccion.Commit();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    ViewBag.Error = "Error al actualizar el producto: " + ex.Message;
                    return View(producto);
                }
            }
        }

        // 5. ELIMINAR PRODUCTO (DELETE)
        public IActionResult Eliminar(int id)
        {
            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                string query = "DELETE FROM productos WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@id", id);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
    }
}