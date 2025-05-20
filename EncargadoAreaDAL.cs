using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Gestor_Desempeno
{

    // Clase para representar una asignación de Encargado a Área
    public class EncargadoAreaInfo
    {
        public int IdEncargadoArea { get; set; }
        public int Id_Area_Ejecutora { get; set; }
        public string Usuario { get; set; }
        // Propiedad adicional para mostrar el nombre del área en el GridView
        public string NombreAreaEjecutora { get; set; }
    }

    public class EncargadoAreaDAL
    {
        // Obtener la cadena de conexión desde Web.config
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }

        // Método para obtener todas las asignaciones, incluyendo el nombre del área
        public List<EncargadoAreaInfo> ObtenerEncargados()
        {
            List<EncargadoAreaInfo> listaEncargados = new List<EncargadoAreaInfo>();
            // Query con JOIN para obtener el nombre del área
            string query = @"SELECT ea.Id_Encargado_Area, ea.Id_Area_Ejecutora, ea.Usuario, ae.Nombre AS NombreAreaEjecutora
                         FROM dbo.Encargado_Area ea
                         INNER JOIN dbo.Area_Ejecutora ae ON ea.Id_Area_Ejecutora = ae.Id_Area_Ejecutora
                         ORDER BY ae.Nombre, ea.Usuario"; // Ordenar por nombre de área y luego usuario

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                EncargadoAreaInfo encargado = new EncargadoAreaInfo
                                {
                                    IdEncargadoArea = Convert.ToInt32(reader["Id_Encargado_Area"]),
                                    Id_Area_Ejecutora = Convert.ToInt32(reader["Id_Area_Ejecutora"]),
                                    Usuario = reader["Usuario"] != DBNull.Value ? reader["Usuario"].ToString() : string.Empty,
                                    NombreAreaEjecutora = reader["NombreAreaEjecutora"] != DBNull.Value ? reader["NombreAreaEjecutora"].ToString() : string.Empty
                                };
                                listaEncargados.Add(encargado);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerEncargados: " + ex.Message);
                        // Considera lanzar la excepción
                        // throw;
                    }
                }
            }
            return listaEncargados;
        }

        // Método para insertar una nueva asignación
        // Devuelve el ID del nuevo registro o -1 si falla
        public int InsertarEncargado(int id_Area_Ejecutora, string usuario)
        {
            int nuevoId = -1;
            // Validar que los parámetros no sean inválidos (usuario no vacío, idArea > 0)
            if (id_Area_Ejecutora <= 0 || string.IsNullOrWhiteSpace(usuario))
            {
                Console.WriteLine("Error en InsertarEncargado: Parámetros inválidos.");
                return -1; // Indicar fallo por parámetros inválidos
            }

            string query = @"INSERT INTO dbo.Encargado_Area (Id_Area_Ejecutora, Usuario)
                         VALUES (@IdAreaEjecutora, @Usuario);
                         SELECT SCOPE_IDENTITY();"; // Obtener el ID insertado

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdAreaEjecutora", id_Area_Ejecutora);
                    cmd.Parameters.AddWithValue("@Usuario", usuario.Trim()); // Guardar sin espacios extra

                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar(); // Ejecutar y obtener el ID
                        if (result != null && result != DBNull.Value)
                        {
                            nuevoId = Convert.ToInt32(result);
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Error SQL en InsertarEncargado: " + ex.Message);
                        // Podrías verificar si es un error de duplicado (si hubiera una constraint unique)
                        return -1; // Indicar fallo
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error general en InsertarEncargado: " + ex.Message);
                        return -1; // Indicar fallo
                    }
                }
            }
            return nuevoId;
        }

        // Método para actualizar una asignación existente
        public bool ActualizarEncargado(int idEncargadoArea, int id_Area_Ejecutora, string usuario)
        {
            bool actualizado = false;
            // Validar que los parámetros no sean inválidos
            if (idEncargadoArea <= 0 || id_Area_Ejecutora <= 0 || string.IsNullOrWhiteSpace(usuario))
            {
                Console.WriteLine("Error en ActualizarEncargado: Parámetros inválidos.");
                return false;
            }

            string query = @"UPDATE dbo.Encargado_Area
                         SET Id_Area_Ejecutora = @IdAreaEjecutora, Usuario = @Usuario
                         WHERE Id_Encargado_Area = @IdEncargadoArea";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdAreaEjecutora", id_Area_Ejecutora);
                    cmd.Parameters.AddWithValue("@Usuario", usuario.Trim());
                    cmd.Parameters.AddWithValue("@IdEncargadoArea", idEncargadoArea);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            actualizado = true;
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Error SQL en ActualizarEncargado: " + ex.Message);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error general en ActualizarEncargado: " + ex.Message);
                        return false;
                    }
                }
            }
            return actualizado;
        }

        // Método para eliminar una asignación específica
        public bool EliminarEncargado(int idEncargadoArea)
        {
            bool eliminado = false;
            if (idEncargadoArea <= 0) return false; // Validar ID

            string query = "DELETE FROM dbo.Encargado_Area WHERE Id_Encargado_Area = @IdEncargadoArea";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdEncargadoArea", idEncargadoArea);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            eliminado = true;
                        }
                    }
                    catch (SqlException ex)
                    {
                        // No debería haber FK constraints que impidan eliminar de esta tabla,
                        // a menos que otras tablas referencien Id_Encargado_Area.
                        Console.WriteLine($"Error SQL al eliminar Encargado_Area: {ex.Message}");
                        throw; // Re-lanzar para que la capa superior lo maneje
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error general al eliminar Encargado_Area: {ex.Message}");
                        throw;
                    }
                }
            }
            return eliminado;
        }

        // Método para obtener una asignación específica por ID (puede ser útil)
        public EncargadoAreaInfo ObtenerEncargadoPorId(int idEncargadoArea)
        {
            EncargadoAreaInfo encargado = null;
            string query = @"SELECT ea.Id_Encargado_Area, ea.Id_Area_Ejecutora, ea.Usuario, ae.Nombre AS NombreAreaEjecutora
                         FROM dbo.Encargado_Area ea
                         INNER JOIN dbo.Area_Ejecutora ae ON ea.Id_Area_Ejecutora = ae.Id_Area_Ejecutora
                         WHERE ea.Id_Encargado_Area = @IdEncargadoArea";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdEncargadoArea", idEncargadoArea);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Si se encontró
                            {
                                encargado = new EncargadoAreaInfo
                                {
                                    IdEncargadoArea = Convert.ToInt32(reader["Id_Encargado_Area"]),
                                    Id_Area_Ejecutora = Convert.ToInt32(reader["Id_Area_Ejecutora"]),
                                    Usuario = reader["Usuario"] != DBNull.Value ? reader["Usuario"].ToString() : string.Empty,
                                    NombreAreaEjecutora = reader["NombreAreaEjecutora"] != DBNull.Value ? reader["NombreAreaEjecutora"].ToString() : string.Empty
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerEncargadoPorId: " + ex.Message);
                    }
                }
            }
            return encargado;
        }

        // Método para verificar si ya existe una asignación para evitar duplicados exactos
        public bool ExisteAsignacion(int idAreaEjecutora, string usuario, int idExcluir = 0)
        {
            bool existe = false;
            string query = @"SELECT COUNT(*) FROM dbo.Encargado_Area
                          WHERE Id_Area_Ejecutora = @Id_Area_Ejecutora AND Usuario = @Usuario";
            // Si se proporciona idExcluir, se usa para verificar al actualizar (no considerar el registro actual)
            if (idExcluir > 0)
            {
                query += " AND Id_Encargado_Area <> @IdExcluir";
            }

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdAreaEjecutora", idAreaEjecutora);
                    cmd.Parameters.AddWithValue("@Usuario", usuario);
                    if (idExcluir > 0)
                    {
                        cmd.Parameters.AddWithValue("@IdExcluir", idExcluir);
                    }

                    try
                    {
                        con.Open();
                        int count = (int)cmd.ExecuteScalar();
                        existe = (count > 0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ExisteAsignacion: " + ex.Message);
                        // Considerar lanzar la excepción
                    }
                }
            }
            return existe;
        }
    }

}