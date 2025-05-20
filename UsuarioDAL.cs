using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration; // Para leer Web.config
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices.AccountManagement;

namespace Gestor_Desempeno
{
    public class UsuarioDAL
    {
        // Obtener la cadena de conexión desde Web.config
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        // Método para validar las credenciales del usuario
        public bool ValidarUsuario(string idUsuario, string contrasena)
        {
            bool isValid = false;
            // ADVERTENCIA: Comparación de contraseña en texto plano como se solicitó. ¡Muy inseguro!
            string query = "SELECT 1 FROM dbo.SEG_USUARIO WHERE ID_USUARIO = @IdUsuario AND CONTRASENA = @Contrasena AND ESTADO = 1";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@Contrasena", contrasena); // Se pasa la contraseña en texto plano

                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar(); // ExecuteScalar es eficiente para verificar existencia
                        if (result != null) // Si devuelve algo (en este caso 1), el usuario es válido y activo
                        {
                            isValid = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Manejar la excepción (p.ej., registrarla)
                        Console.WriteLine("Error en ValidarUsuario: " + ex.Message);
                        // Podrías lanzar la excepción o devolver false según tu manejo de errores
                        isValid = false;
                    }
                }
            }
            return isValid;
        }

        // En UsuarioDAL.cs
        public UsuarioInfo ObtenerInfoUsuario(string idUsuario)
        {
            UsuarioInfo info = null;
            // Incluir CORREO y DebeCambiarContrasena en la consulta
            string query = @"SELECT ID_USUARIO, CONTRASENA, ES_EXTERNO, ESTADO, CORREO, DebeCambiarContrasena 
                     FROM dbo.SEG_USUARIO 
                     WHERE ID_USUARIO = @IdUsuario";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                info = new UsuarioInfo
                                {
                                    IdUsuario = reader["ID_USUARIO"].ToString(),
                                    ContrasenaAlmacenada = reader["CONTRASENA"] != DBNull.Value ? reader["CONTRASENA"].ToString() : null,
                                    EsExterno = Convert.ToBoolean(reader["ES_EXTERNO"]),
                                    EstaActivo = Convert.ToBoolean(reader["ESTADO"]),
                                    // Mapear nuevos campos
                                    Correo = reader["CORREO"] != DBNull.Value ? reader["CORREO"].ToString() : null,
                                    NecesitaCambiarContrasena = Convert.ToBoolean(reader["DebeCambiarContrasena"])
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerInfoUsuario: " + ex.Message);
                    }
                }
            }
            return info;
        }

        // En UsuarioDAL.cs
        // ADVERTENCIA: Almacena la contraseña temporal en texto plano.
        public bool ActualizarContrasenaYTempora(string idUsuario, string nuevaContrasenaTemporal)
        {
            bool actualizado = false;
            // Actualiza CONTRASENA y pone DebeCambiarContrasena en 1 (true)
            string query = @"UPDATE dbo.SEG_USUARIO 
                     SET CONTRASENA = @NuevaContrasena, 
                         DebeCambiarContrasena = 1 
                     WHERE ID_USUARIO = @IdUsuario";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@NuevaContrasena", nuevaContrasenaTemporal); // Texto plano
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            actualizado = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ActualizarContrasenaYTempora: " + ex.Message);
                    }
                }
            }
            return actualizado;
        }

        // En UsuarioDAL.cs
        // ADVERTENCIA: Almacena la nueva contraseña en texto plano.
        public bool ActualizarContrasenaYDesactivarForzado(string idUsuario, string nuevaContrasena)
        {
            bool actualizado = false;
            // Actualiza CONTRASENA y pone DebeCambiarContrasena en 0 (false)
            string query = @"UPDATE dbo.SEG_USUARIO 
                     SET CONTRASENA = @NuevaContrasena, 
                         DebeCambiarContrasena = 0 
                     WHERE ID_USUARIO = @IdUsuario";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@NuevaContrasena", nuevaContrasena); // Texto plano
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            actualizado = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ActualizarContrasenaYDesactivarForzado: " + ex.Message);
                    }
                }
            }
            return actualizado;
        }

        // En UsuarioDAL.cs
        public string GenerarContrasenaTemporal(int longitud = 10) // Genera contraseña de 10 caracteres por defecto
        {
            // Caracteres posibles (excluye caracteres ambiguos como I, l, 1, O, 0)
            const string chars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, longitud)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        // Nuevo método para validar contra AD
        public bool ValidarConActiveDirectory(string idUsuario, string contrasena)
        {
            // --- Configuración de Active Directory ---
            // Obtén el nombre de tu dominio de AD. Es mejor ponerlo en Web.config
            string domainName = ConfigurationManager.AppSettings["ActiveDirectoryDomain"];
            if (string.IsNullOrEmpty(domainName))
            {
                Console.WriteLine("Error: La clave 'ActiveDirectoryDomain' no está configurada en appSettings de Web.config.");
                return false; // O lanza una excepción de configuración
            }
            // -----------------------------------------

            bool isValid = false;
            try
            {
                // Usar ContextType.Domain para un dominio de AD estándar
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domainName))
                {
                    // ValidateCredentials intentará autenticar al usuario contra el dominio
                    isValid = pc.ValidateCredentials(idUsuario, contrasena);
                }
            }
            catch (PrincipalServerDownException ex)
            {
                // Error común: No se puede contactar al controlador de dominio
                Console.WriteLine($"Error de AD: No se pudo conectar al dominio '{domainName}'. Verifica la conectividad y el nombre del dominio. Detalles: {ex.Message}");
                // Aquí podrías querer lanzar una excepción o registrar el error detallado
                isValid = false;
            }
            catch (Exception ex)
            {
                // Otros errores posibles durante la validación de AD
                Console.WriteLine($"Error inesperado durante la validación con Active Directory para el usuario '{idUsuario}': {ex.Message}");
                // Registrar el error detallado
                isValid = false;
            }
            return isValid;
        }

        // Método para obtener el correo de un usuario (para olvido de contraseña)
        public string ObtenerCorreoPorUsuario(string idUsuario)
        {
            string correo = null;
            string query = "SELECT CORREO FROM dbo.SEG_USUARIO WHERE ID_USUARIO = @IdUsuario AND ESTADO = 1"; // Asegurarse que esté activo

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            correo = result.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerCorreoPorUsuario: " + ex.Message);
                        // Manejar la excepción
                    }
                }
            }
            return correo;
        }

        // --- Métodos Adicionales (Opcional - Para recuperación completa) ---

        // Método para actualizar la contraseña (¡Almacena en texto plano!)
        public bool ActualizarContrasena(string idUsuario, string nuevaContrasena)
        {
            // ADVERTENCIA: Almacenando nueva contraseña en texto plano. ¡Muy inseguro!
            bool actualizado = false;
            string query = "UPDATE dbo.SEG_USUARIO SET CONTRASENA = @NuevaContrasena WHERE ID_USUARIO = @IdUsuario";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@NuevaContrasena", nuevaContrasena);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            actualizado = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ActualizarContrasena: " + ex.Message);
                        // Manejar la excepción
                    }
                }
            }
            return actualizado;
        }
    }
}