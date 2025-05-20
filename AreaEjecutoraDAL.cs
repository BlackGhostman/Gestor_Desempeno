using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Gestor_Desempeno
{


    // Clase para representar un Area Ejecutora
    public class AreaEjecutoraInfo
    {
        public int Id_Area_Ejecutora { get; set; } // Matches the C# convention
        public string Nombre { get; set; }
    }

    public class AreaEjecutoraDAL
    {
        // Obtener la cadena de conexión desde Web.config
        private string GetConnectionString()
        {
            // Asegúrate de usar el nombre correcto de la connection string para Objetivos_Metas
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }

        // Método para obtener todas las áreas ejecutoras
        public List<AreaEjecutoraInfo> ObtenerAreas()
        {
            List<AreaEjecutoraInfo> listaAreas = new List<AreaEjecutoraInfo>();
            // No hay columna 'Estado', así que obtenemos todas
            string query = "SELECT Id_Area_Ejecutora, Nombre FROM dbo.Area_Ejecutora ORDER BY Nombre";

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
                                AreaEjecutoraInfo area = new AreaEjecutoraInfo
                                {
                                    // Use C# property names for mapping
                                    Id_Area_Ejecutora = Convert.ToInt32(reader["Id_Area_Ejecutora"]),
                                    Nombre = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString() : string.Empty
                                };
                                listaAreas.Add(area);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerAreas: " + ex.Message);
                        // Considera lanzar la excepción o manejarla de otra forma
                        // throw;
                    }
                }
            }
            return listaAreas;
        }

        // Método para insertar una nueva área ejecutora
        public bool InsertarArea(string nombre)
        {
            bool insertado = false;
            string query = "INSERT INTO dbo.Area_Ejecutora (Nombre) VALUES (@Nombre)";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre ?? (object)DBNull.Value);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            insertado = true;
                        }
                    }
                    catch (SqlException ex) // Catch specific SQL exceptions
                    {
                        Console.WriteLine("Error SQL en InsertarArea: " + ex.Message);
                        // Check for specific errors like unique constraints if needed
                        // if (ex.Number == 2627) // Unique constraint violation
                        // {
                        //    // Handle appropriately
                        // }
                        return false; // Indicate failure
                    }
                    catch (Exception ex) // Catch general exceptions
                    {
                        Console.WriteLine("Error general en InsertarArea: " + ex.Message);
                        return false; // Indicate failure
                    }
                }
            }
            return insertado;
        }

        // Método para actualizar un área ejecutora existente
        public bool ActualizarArea(int idAreaEjecutora, string nombre)
        {
            bool actualizado = false;
            string query = "UPDATE dbo.Area_Ejecutora SET Nombre = @Nombre WHERE Id_Area_Ejecutora = @Id_Area_Ejecutora";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_Area_Ejecutora", idAreaEjecutora);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            actualizado = true;
                        }
                    }
                    catch (SqlException ex) // Catch specific SQL exceptions
                    {
                        Console.WriteLine("Error SQL en ActualizarArea: " + ex.Message);
                        return false; // Indicate failure
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error general en ActualizarArea: " + ex.Message);
                        return false; // Indicate failure
                    }
                }
            }
            return actualizado;
        }

        // Método para eliminar FÍSICAMENTE un área ejecutora
        // ADVERTENCIA: Esto eliminará permanentemente el registro.
        // Puede fallar si existen registros relacionados en otras tablas (Encargado_Area, Meta_Departamental)
        // debido a restricciones de clave externa (Foreign Key).
        public bool EliminarArea(int idAreaEjecutora)
        {
            bool eliminado = false;
            string query = "DELETE FROM dbo.Area_Ejecutora WHERE Id_Area_Ejecutora = @Id_Area_Ejecutora";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id_Area_Ejecutora", idAreaEjecutora);

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
                        // Error común: Violación de restricción FOREIGN KEY
                        if (ex.Number == 547)
                        {
                            Console.WriteLine($"Error al eliminar Area Ejecutora (ID: {idAreaEjecutora}): Existen registros dependientes en otras tablas.");
                            // Podrías lanzar una excepción personalizada o devolver un código de error específico
                            throw new InvalidOperationException("No se puede eliminar el área ejecutora porque tiene registros asociados (encargados o metas departamentales).", ex);
                        }
                        else
                        {
                            Console.WriteLine($"Error SQL al eliminar Area Ejecutora: {ex.Message}");
                            throw; // Re-lanzar otras excepciones SQL
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error general al eliminar Area Ejecutora: {ex.Message}");
                        throw; // Re-lanzar otras excepciones
                    }
                }
            }
            return eliminado;
        }

        // Método para obtener un área específica por ID (útil para la edición)
        public AreaEjecutoraInfo ObtenerAreaPorId(int idAreaEjecutora)
        {
            AreaEjecutoraInfo area = null;
            string query = "SELECT Id_Area_Ejecutora, Nombre FROM dbo.Area_Ejecutora WHERE Id_Area_Ejecutora = @Id_Area_Ejecutora";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id_Area_Ejecutora", idAreaEjecutora);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Si se encontró el area
                            {
                                area = new AreaEjecutoraInfo
                                {
                                    Id_Area_Ejecutora = Convert.ToInt32(reader["Id_Area_Ejecutora"]),
                                    Nombre = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString() : string.Empty,
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerAreaPorId: " + ex.Message);
                        // Considera lanzar la excepción o manejarla de otra forma
                    }
                }
            }
            return area;
        }
    }

}