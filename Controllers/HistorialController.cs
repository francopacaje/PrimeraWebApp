using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PrimeraWebApp.Models;

namespace PrimeraWebApp.Controllers
{
    public class HistorialController : Controller
    {
        private readonly string _connectionString;

        public HistorialController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            List<Historial> listaHistorial = new List<Historial>();

            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                // Unimos con productos para ver el nombre del producto en vez de solo el ID
                string query = "SELECT h.id_historial, p.nombre AS NombreProducto, h.cantidad_movimiento, h.tipo_movimiento, h.fecha_hora " +
                               "FROM historial h " +
                               "INNER JOIN productos p ON h.id_producto = p.id " +
                               "ORDER BY h.fecha_hora DESC";

                MySqlCommand cmd = new MySqlCommand(query, conexion);

                try
                {
                    conexion.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listaHistorial.Add(new Historial
                            {
                                IdHistorial = Convert.ToInt32(reader["id_historial"]),
                                NombreProducto = reader["NombreProducto"].ToString(),
                                CantidadMovimiento = Convert.ToInt32(reader["cantidad_movimiento"]),
                                TipoMovimiento = reader["tipo_movimiento"].ToString(),
                                FechaHora = Convert.ToDateTime(reader["fecha_hora"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al obtener historial: " + ex.Message;
                }
            }

            return View(listaHistorial);
        }
    }
}