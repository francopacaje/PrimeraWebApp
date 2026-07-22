using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PrimeraWebApp.Models;

namespace PrimeraWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // MOSTRAR VISTA DE LOGIN
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // PROCESAR EL LOGIN
        [HttpPost]
        public IActionResult Login(string correo, string contrasena)
        {
            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                string query = "SELECT id_usuario, nombre, rol FROM usuario WHERE correo = @correo AND contrasena = @contrasena";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@contrasena", contrasena);

                try
                {
                    conexion.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // ¡LOGIN CORRECTO! Guardamos datos en la Sesión
                            HttpContext.Session.SetInt32("UsuarioId", Convert.ToInt32(reader["id_usuario"]));
                            HttpContext.Session.SetString("UsuarioNombre", reader.GetString("nombre"));
                            HttpContext.Session.SetString("UsuarioRol", reader.GetString("rol"));

                            // CAMBIO AQUÍ: Ahora redirige al Home (Menú de Tarjetas) en vez de Productos
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ViewBag.Error = "Correo o contraseña incorrectos.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error de conexión: " + ex.Message;
                }
            }
            return View();
        }

        // CERRAR SESIÓN
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Limpia la sesión
            return RedirectToAction("Login");
        }
    }
}