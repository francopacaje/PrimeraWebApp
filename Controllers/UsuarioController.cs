using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PrimeraWebApp.Models;

namespace PrimeraWebApp.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly string _connectionString;

        public UsuarioController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // LISTAR USUARIOS
        public IActionResult Index()
        {
            List<Usuario> lista = new List<Usuario>();

            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                string query = "SELECT id_usuario, nombre, correo, contrasena, rol FROM usuario";
                MySqlCommand cmd = new MySqlCommand(query, conexion);

                try
                {
                    conexion.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Usuario
                            {
                                IdUsuario = Convert.ToInt32(reader["id_usuario"]),
                                Nombre = reader.GetString("nombre"),
                                Correo = reader.GetString("correo"),
                                Contrasena = reader.GetString("contrasena"),
                                Rol = reader.GetString("rol")
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al listar: " + ex.Message;
                }
            }
            return View(lista);
        }

        // INSERTAR USUARIO (Seguro)
        [HttpPost]
        public IActionResult Create(Usuario u)
        {
            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                string query = "INSERT INTO usuario (nombre, correo, contrasena, rol) VALUES (@nombre, @correo, @contrasena, @rol)";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@nombre", u.Nombre);
                cmd.Parameters.AddWithValue("@correo", u.Correo);
                cmd.Parameters.AddWithValue("@contrasena", u.Contrasena); // En la fase de Login le ponemos Hash, ahora va directo para probar rápido
                cmd.Parameters.AddWithValue("@rol", u.Rol);

                try
                {
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al guardar: " + ex.Message;
                }
            }
            return RedirectToAction("Index");
        }
    }
}
