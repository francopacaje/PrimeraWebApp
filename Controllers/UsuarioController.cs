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

        // 1. LISTAR USUARIOS (INDEX)
        public IActionResult Index()
        {
            List<Usuario> lista = new List<Usuario>();

            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                string query = "SELECT id_usuario, nombre, correo, rol FROM usuario";
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
                                Nombre = reader["nombre"].ToString(),
                                Correo = reader["correo"].ToString(),
                                Rol = reader["rol"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al obtener usuarios: " + ex.Message;
                }
            }
            return View(lista);
        }

        // 2. CREAR USUARIO (GET Y POST)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Usuario usuario)
        {
            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                string query = "INSERT INTO usuario (nombre, correo, contrasena, rol) VALUES (@nombre, @correo, @contrasena, @rol)";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@correo", usuario.Correo);
                cmd.Parameters.AddWithValue("@contrasena", usuario.Contrasena);
                cmd.Parameters.AddWithValue("@rol", usuario.Rol ?? "Usuario");

                try
                {
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al crear usuario: " + ex.Message;
                    return View(usuario);
                }
            }
        }

        // 3. EDITAR USUARIO (GET Y POST)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Usuario usuario = null;

            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                string query = "SELECT id_usuario, nombre, correo, contrasena, rol FROM usuario WHERE id_usuario = @id";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@id", id);

                try
                {
                    conexion.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                IdUsuario = Convert.ToInt32(reader["id_usuario"]),
                                Nombre = reader["nombre"].ToString(),
                                Correo = reader["correo"].ToString(),
                                Contrasena = reader["contrasena"].ToString(),
                                Rol = reader["rol"].ToString()
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al cargar usuario: " + ex.Message;
                }
            }

            if (usuario == null) return RedirectToAction("Index");
            return View(usuario);
        }

        [HttpPost]
        public IActionResult Edit(Usuario usuario)
        {
            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                string query = "UPDATE usuario SET nombre = @nombre, correo = @correo, contrasena = @contrasena, rol = @rol WHERE id_usuario = @id";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@id", usuario.IdUsuario);
                cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@correo", usuario.Correo);
                cmd.Parameters.AddWithValue("@contrasena", usuario.Contrasena);
                cmd.Parameters.AddWithValue("@rol", usuario.Rol);

                try
                {
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al editar usuario: " + ex.Message;
                    return View(usuario);
                }
            }
        }

        // 4. ELIMINAR USUARIO
        public IActionResult Delete(int id)
        {
            using (MySqlConnection conexion = new MySqlConnection(_connectionString))
            {
                string query = "DELETE FROM usuario WHERE id_usuario = @id";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@id", id);

                try
                {
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "No se puede eliminar el usuario porque tiene registros asociados.";
                }
            }
            return RedirectToAction("Index");
        }
    }
}