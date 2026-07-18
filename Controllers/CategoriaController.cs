using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PrimeraWebApp.Models;

namespace PrimeraWebApp.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly string _connectionString;

        public CategoriaController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // 1. LISTAR CATEGORÍAS
        public IActionResult Index()
        {
            List<Categoria> listaCategorias = new List<Categoria>();

            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                string query = "SELECT id_categoria, nombre FROM categoria";
                MySqlCommand cmd = new MySqlCommand(query, conexion);

                try
                {
                    conexion.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listaCategorias.Add(new Categoria
                            {
                                IdCategoria = reader.GetInt32("id_categoria"),
                                Nombre = reader.GetString("nombre")
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al listar categorías: " + ex.Message;
                }
            }
            return View(listaCategorias);
        }

        // 2. INSERTAR CATEGORÍA (POST Seguro)
        [HttpPost]
        public IActionResult Create(Categoria categoria)
        {
            if (string.IsNullOrEmpty(categoria.Nombre))
            {
                ViewBag.Error = "El nombre de la categoría no puede estar vacío.";
                return RedirectToAction("Index");
            }

            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                // REGLA DE SEGURIDAD: Uso de parámetro seguro @nombre
                string query = "INSERT INTO categoria (nombre) VALUES (@nombre)";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@nombre", categoria.Nombre);

                try
                {
                    conexion.Open();
                    cmd.ExecuteNonQuery(); // Modifica la BD sin retornar filas
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al insertar categoría: " + ex.Message;
                }
            }

            return RedirectToAction("Index");
        }
    }
}
