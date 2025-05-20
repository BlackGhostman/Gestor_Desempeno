using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Gestor_Desempeno
{


    public class ClaseInfo
    {
        public int IdClase { get; set; }
        public string Nombre { get; set; } // nvarchar(50)
    }

    public class ClaseDAL
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }

        // Obtener todas las clases
        public List<ClaseInfo> ObtenerClases()
        {
            List<ClaseInfo> lista = new List<ClaseInfo>();
            string query = "SELECT Id_Clase, Nombre FROM dbo.Clase ORDER BY Nombre";

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
                                lista.Add(new ClaseInfo
                                {
                                    IdClase = Convert.ToInt32(reader["Id_Clase"]),
                                    Nombre = reader["Nombre"]?.ToString() ?? string.Empty
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerClases: " + ex.Message);
                        throw; // Re-throw exception
                    }
                }
            }
            return lista;
        }

        // Método para insertar una nueva clase
        public int InsertarClase(string nombre)
        {
            int nuevoId = -1;
            if (string.IsNullOrWhiteSpace(nombre)) return -1; // Basic validation

            // Check if name already exists (case-insensitive check recommended for robustness)
            if (ExisteClase(nombre))
            {
                Console.WriteLine($"Error en InsertarClase: Ya existe una clase con el nombre '{nombre}'.");
                throw new InvalidOperationException($"Ya existe una clase con el nombre '{nombre}'."); // Throw specific error
            }


            string query = @"INSERT INTO dbo.Clase (Nombre) VALUES (@Nombre);
                         SELECT SCOPE_IDENTITY();";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre.Trim());

                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            nuevoId = Convert.ToInt32(result);
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Error SQL en InsertarClase: " + ex.Message);
                        // Could be a unique constraint violation if added to DB later
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error general en InsertarClase: " + ex.Message);
                        throw; // Re-throw
                    }
                }
            }
            return nuevoId;
        }

        // Método para actualizar una clase existente
        public bool ActualizarClase(int idClase, string nombre)
        {
            bool actualizado = false;
            if (idClase <= 0 || string.IsNullOrWhiteSpace(nombre)) return false; // Validation

            // Check if name already exists (excluding the current record)
            if (ExisteClase(nombre, idClase))
            {
                Console.WriteLine($"Error en ActualizarClase: Ya existe otra clase con el nombre '{nombre}'.");
                throw new InvalidOperationException($"Ya existe otra clase con el nombre '{nombre}'."); // Throw specific error
            }


            string query = "UPDATE dbo.Clase SET Nombre = @Nombre WHERE Id_Clase = @IdClase";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdClase", idClase);
                    cmd.Parameters.AddWithValue("@Nombre", nombre.Trim());

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        actualizado = (rowsAffected > 0);
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Error SQL en ActualizarClase: " + ex.Message);
                        throw; // Re-throw
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error general en ActualizarClase: " + ex.Message);
                        throw; // Re-throw
                    }
                }
            }
            return actualizado;
        }

        // Método para eliminar FÍSICAMENTE una clase
        // ADVERTENCIA: Fallará si la clase está en uso por Detalle_Estado (FK constraint)
        public bool EliminarClase(int idClase)
        {
            if (idClase <= 0) return false; // Validation
            bool eliminado = false;
            string query = "DELETE FROM dbo.Clase WHERE Id_Clase = @IdClase";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdClase", idClase);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        eliminado = (rowsAffected > 0);
                    }
                    catch (SqlException ex)
                    {
                        // Error común: Violación de FK si la clase está en uso
                        if (ex.Number == 547)
                        {
                            Console.WriteLine($"Error al eliminar Clase (ID: {idClase}): La clase está siendo utilizada por Detalles de Estado.");
                            throw new InvalidOperationException("No se puede eliminar la clase porque tiene estados asociados.", ex);
                        }
                        else
                        {
                            Console.WriteLine($"Error SQL al eliminar Clase: {ex.Message}");
                            throw; // Re-lanzar otras excepciones SQL
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error general al eliminar Clase: {ex.Message}");
                        throw;
                    }
                }
            }
            return eliminado;
        }

        // Método para obtener una clase específica por ID
        public ClaseInfo ObtenerClasePorId(int idClase)
        {
            ClaseInfo clase = null;
            string query = "SELECT Id_Clase, Nombre FROM dbo.Clase WHERE Id_Clase = @IdClase";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdClase", idClase);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Si se encontró
                            {
                                clase = new ClaseInfo
                                {
                                    IdClase = Convert.ToInt32(reader["Id_Clase"]),
                                    Nombre = reader["Nombre"]?.ToString() ?? string.Empty
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerClasePorId: " + ex.Message);
                        // Consider throwing exception
                    }
                }
            }
            return clase;
        }

        // Método para verificar si ya existe una clase con el mismo nombre (case-insensitive)
        public bool ExisteClase(string nombre, int idExcluir = 0)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return false;
            bool existe = false;
            // Use UPPER for case-insensitive comparison
            string query = "SELECT COUNT(*) FROM dbo.Clase WHERE UPPER(Nombre) = UPPER(@Nombre)";
            // Excluir el registro actual si estamos actualizando
            if (idExcluir > 0)
            {
                query += " AND Id_Clase <> @IdExcluir";
            }

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre.Trim());
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
                        Console.WriteLine("Error en ExisteClase: " + ex.Message);
                        // Consider throwing exception
                    }
                }
            }
            return existe;
        }
    }


}