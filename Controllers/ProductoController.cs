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
                // Consulta limpia respetando tu INNER JOIN con la columna 'categoria'
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
                                // CORRECCIÓN DE SINTAXIS/TIPO: Convertimos explícitamente para evitar caídas si el precio usa decimales o int en MySQL
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

        // Vista para cargar el formulario
        public IActionResult Create()
        {
            return View();
        }

        // 2. INSERTAR PRODUCTO E HISTORIAL (Operación Crítica de la Fase 3)
        [HttpPost]
        public IActionResult Create(Producto producto)
        {
            // Validar que el stock no sea negativo (Requisito Obligatorio de la Fase 3 - Contexto 1.b)
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

                    // PASO A: Insertar el Producto usando comandos parametrizados seguros (Evita Inyección SQL - Punto 2)
                    string queryProducto = "INSERT INTO productos (nombre, precio, stock, categoria) " +
                                           "VALUES (@nombre, @precio, @stock, @categoria); " +
                                           "SELECT LAST_INSERT_ID();";

                    MySqlCommand cmdProd = new MySqlCommand(queryProducto, conexion);
                    cmdProd.Parameters.AddWithValue("@nombre", producto.Nombre);
                    cmdProd.Parameters.AddWithValue("@precio", producto.Precio);
                    cmdProd.Parameters.AddWithValue("@stock", producto.Stock);
                    cmdProd.Parameters.AddWithValue("@categoria", producto.IdCategoria); // Parámetro y nombre alineados correctamente

                    // Usamos ExecuteScalar() para obtener de inmediato el ID numérico asignado por la base de datos
                    int idProductoReciente = Convert.ToInt32(cmdProd.ExecuteScalar());

                    // PASO B: Insertar de manera automática el registro en la tabla 'historial' (Punto 4.a del Cliente)
                    // Usamos NOW() de MySQL para la fecha y hora exacta del servidor
                    string queryHistorial = "INSERT INTO historial (id_producto, cantidad_movimiento, tipo_movimiento, fecha_hora) " +
                                            "VALUES (@id_producto, @cantidad, @tipo, NOW())";

                    MySqlCommand cmdHist = new MySqlCommand(queryHistorial, conexion);
                    cmdHist.Parameters.AddWithValue("@id_producto", idProductoReciente);
                    cmdHist.Parameters.AddWithValue("@cantidad", producto.Stock);
                    cmdHist.Parameters.AddWithValue("@tipo", "INGRESO INICIAL");

                    // Criterio de éxito 3.c: Modifica datos sin retornar filas
                    cmdHist.ExecuteNonQuery();

                    // Criterio de éxito 3.d: Refrescar la página redirigiendo al listado inmediatamente
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