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



        public bool CambiarContrasenaActiveDirectory(string idUsuario, string contrasenaActual, string nuevaContrasena)
        {
            string domainName = ConfigurationManager.AppSettings["ActiveDirectoryDomain"];
            if (string.IsNullOrEmpty(domainName))
            {
                Console.WriteLine("Error: La clave 'ActiveDirectoryDomain' no está configurada.");
                return false;
            }

            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domainName))
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, idUsuario);
                    if (user != null)
                    {
                        // El método ChangePassword requiere la contraseña actual.
                        user.ChangePassword(contrasenaActual, nuevaContrasena);
                        // Si el cambio es exitoso y estás almacenando el flag `DebeCambiarContrasena`
                        // localmente, asegúrate de resetearlo aquí en tu base de datos.
                        // ActualizarContrasenaYDesactivarForzado(idUsuario, ""); // Podrías pasar la nueva contraseña (hasheada!) o simplemente resetear el flag
                        // Idealmente, no almacenes la contraseña de AD en tu BD local.
                        // Solo resetea el flag:
                        // MarcarUsuarioParaCambioContrasenaEnDB(idUsuario, false); // Método hipotético
                        return true;
                    }
                }
            }
            catch (PasswordException ex)
            {
                // Contraseña no cumple políticas de complejidad, historial, etc.
                Console.WriteLine($"Error de contraseña en AD para '{idUsuario}': {ex.Message}");
                // Puedes lanzar una excepción personalizada con un mensaje amigable
                throw new Exception("La nueva contraseña no cumple con las políticas de seguridad del dominio (ej: complejidad, historial).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cambiar contraseña en AD para '{idUsuario}': {ex.Message}");
                // Considera un manejo de errores más específico si es necesario
                throw; // Relanza para que la capa superior lo maneje
            }
            return false;
        }

        // Alternativa si la contraseña está expirada y AD permite SetPassword (generalmente necesita permisos elevados o es el propio usuario autenticado)
        // Este método NO requiere la contraseña antigua, pero debe usarse con precaución.
        public bool ForzarCambioContrasenaActiveDirectory(string idUsuario, string nuevaContrasena)
        {
            string domainName = ConfigurationManager.AppSettings["ActiveDirectoryDomain"];
            // ... (similar al anterior) ...
            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domainName))
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, idUsuario);
                    if (user != null)
                    {
                        user.SetPassword(nuevaContrasena); // Establece directamente la contraseña.
                                                           // Útil si "User must change password at next logon" está activo.
                                                           // Asegúrate de que la cuenta no esté bloqueada y que esté habilitada.
                                                           // user.UnlockAccount(); // Si estuviera bloqueada
                                                           // user.ExpirePasswordNow(); // Para forzar el cambio, aunque ya estamos cambiándola.

                        // Después de un SetPassword exitoso, el flag "User must change password at next logon"
                        // generalmente se desactiva automáticamente en AD.
                        // Actualiza tu flag local en la BD:
                        // MarcarUsuarioParaCambioContrasenaEnDB(idUsuario, false); // Método hipotético
                        return true;
                    }
                }
            }
            // ... (manejo de excepciones similar, PasswordException puede ocurrir por políticas) ...
            catch (PasswordException ex)
            {
                Console.WriteLine($"Error de contraseña en AD para '{idUsuario}': {ex.Message}");
                throw new Exception("La nueva contraseña no cumple con las políticas de seguridad del dominio.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al forzar cambio de contraseña en AD para '{idUsuario}': {ex.Message}");
                throw;
            }
            return false;
        }


        // Método hipotético para actualizar solo el flag en tu BD
        public void MarcarUsuarioParaCambioContrasenaEnDB(string idUsuario, bool debeCambiar)
        {
            string query = @"UPDATE dbo.SEG_USUARIO SET DebeCambiarContrasena = @DebeCambiar
                     WHERE ID_USUARIO = @IdUsuario";
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@DebeCambiar", debeCambiar);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en MarcarUsuarioParaCambioContrasenaEnDB: " + ex.Message);
                    }
                }
            }
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

                // Necesitarás obtener el UserPrincipal para verificar el estado de la contraseña en AD
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domainName))
                {
                    if (pc.ValidateCredentials(idUsuario, contrasena))
                    {
                        // Autenticación exitosa, ahora obtener el UserPrincipal
                        UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, idUsuario);
                        if (userPrincipal != null)
                        {
                            // Verificar si la contraseña ha expirado o si se requiere cambio
                            bool adRequiereCambio = false;
                            if (userPrincipal.LastPasswordSet == null) // Común si "User must change password at next logon" está marcado
                            {
                                adRequiereCambio = true;
                            }
                            else
                            {
                                // También puedes verificar userPrincipal.IsAccountLockedOut(), userPrincipal.Enabled, etc.
                                // Para verificar la expiración real, necesitarías comparar userPrincipal.LastPasswordSet
                                // con la política de expiración de contraseñas del dominio.
                                // Una forma más directa es intentar acceder a userPrincipal.PasswordExpired (puede que no esté disponible en todas las versiones o configuraciones)
                                // o verificar el atributo 'msDS-UserPasswordExpired' directamente vía DirectoryEntry si es necesario.
                                // De forma sencilla, si AD te obliga a cambiarla, a veces ValidateCredentials falla de forma específica o userPrincipal.PasswordNotRequired = false y userPrincipal.PasswordNeverExpires = false pueden dar pistas.
                                // La detección más robusta de "expirado" puede requerir leer atributos específicos de AD.
                                // Por ahora, si "User must change password at next logon" es tu principal caso de uso, userPrincipal.LastPasswordSet == null es un buen indicador.
                            }

                            // Si adRequiereCambio es true, actualiza tu tabla SEG_USUARIO para ese usuario
                            // Establece DebeCambiarContrasena = 1 para redirigirlo a CambiarContrasena.aspx
                            // Este es un ejemplo, adapta la lógica para actualizar tu BD:
                            // if (adRequiereCambio) {
                            //     MarcarUsuarioParaCambioContrasenaEnDB(idUsuario, true);
                            // }
                        }
                    }
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