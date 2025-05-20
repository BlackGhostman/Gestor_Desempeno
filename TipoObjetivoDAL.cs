using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Gestor_Desempeno
{

    public class TipoObjetivoInfo
    {
        public int Id_Tipo_Objetivo { get; set; }
        public string Nombre { get; set; } // Asumiendo que Nombre(10) es suficiente
        public string Descripcion { get; set; }
        public bool Estado { get; set; }
    }

    public class TipoObjetivoDAL
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }

        // Obtener tipos de objetivo (filtrando por activos o todos)
        public List<TipoObjetivoInfo> ObtenerTiposObjetivo(bool soloActivos = true)
        {
            List<TipoObjetivoInfo> lista = new List<TipoObjetivoInfo>();
            string query = "SELECT Id_Tipo_Objetivo, Nombre, Descripcion, Estado FROM dbo.Tipo_Objetivo";
            if (soloActivos)
            {
                query += " WHERE Estado = 1"; // Asume columna Estado existe
            }
            query += " ORDER BY Nombre";


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
                                lista.Add(new TipoObjetivoInfo
                                {
                                    Id_Tipo_Objetivo = Convert.ToInt32(reader["Id_Tipo_Objetivo"]),
                                    Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                                    Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                                    Estado = Convert.ToBoolean(reader["Estado"]) // Asume no es null
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerTiposObjetivo: " + ex.Message);
                        // Consider throwing exception
                        throw;
                    }
                }
            }
            return lista;
        }

        // Método para insertar un nuevo tipo de objetivo
        public int InsertarTipoObjetivo(string nombre, string descripcion, bool estado)
        {
            int nuevoId = -1;
            if (string.IsNullOrWhiteSpace(nombre)) return -1; // Validación básica

            string query = @"INSERT INTO dbo.Tipo_Objetivo (Nombre, Descripcion, Estado)
                         VALUES (@Nombre, @Descripcion, @Estado);
                         SELECT SCOPE_IDENTITY();";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre.Trim());
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : (object)descripcion.Trim());
                    cmd.Parameters.AddWithValue("@Estado", estado);

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
                        Console.WriteLine("Error SQL en InsertarTipoObjetivo: " + ex.Message);
                        // Check for unique constraint if needed (ex.Number == 2627)
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error general en InsertarTipoObjetivo: " + ex.Message);
                        return -1;
                    }
                }
            }
            return nuevoId;
        }

        // Método para actualizar un tipo de objetivo existente
        public bool ActualizarTipoObjetivo(int id_Tipo_Objetivo, string nombre, string descripcion, bool estado)
        {
            bool actualizado = false;
            if (id_Tipo_Objetivo <= 0 || string.IsNullOrWhiteSpace(nombre)) return false; // Validación

            string query = @"UPDATE dbo.Tipo_Objetivo SET
                            Nombre = @Nombre,
                            Descripcion = @Descripcion,
                            Estado = @Estado
                         WHERE Id_Tipo_Objetivo = @Id_Tipo_Objetivo";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id_Tipo_Objetivo", id_Tipo_Objetivo);
                    cmd.Parameters.AddWithValue("@Nombre", nombre.Trim());
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : (object)descripcion.Trim());
                    cmd.Parameters.AddWithValue("@Estado", estado);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        actualizado = (rowsAffected > 0);
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Error SQL en ActualizarTipoObjetivo: " + ex.Message);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error general en ActualizarTipoObjetivo: " + ex.Message);
                        return false;
                    }
                }
            }
            return actualizado;
        }

        // Método para eliminar lógicamente un tipo de objetivo (cambiar Estado a false)
        public bool EliminarTipoObjetivoLogico(int id_Tipo_Objetivo)
        {
            if (id_Tipo_Objetivo <= 0) return false; // Validación
            bool eliminado = false;
            string query = "UPDATE dbo.Tipo_Objetivo SET Estado = 0 WHERE Id_Tipo_Objetivo = @Id_Tipo_Objetivo";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id_Tipo_Objetivo", id_Tipo_Objetivo);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        eliminado = (rowsAffected > 0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en EliminarTipoObjetivoLogico: " + ex.Message);
                        // Consider throwing exception
                        throw;
                    }
                }
            }
            return eliminado;
        }

        // Método para obtener un tipo de objetivo específico por ID
        public TipoObjetivoInfo ObtenerTipoObjetivoPorId(int id_Tipo_Objetivo)
        {
            TipoObjetivoInfo tipo = null;
            string query = "SELECT Id_Tipo_Objetivo, Nombre, Descripcion, Estado FROM dbo.Tipo_Objetivo WHERE Id_Tipo_Objetivo = @Id_Tipo_Objetivo";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id_Tipo_Objetivo", id_Tipo_Objetivo);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Si se encontró
                            {
                                tipo = new TipoObjetivoInfo
                                {
                                    Id_Tipo_Objetivo = Convert.ToInt32(reader["Id_Tipo_Objetivo"]),
                                    Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                                    Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                                    Estado = Convert.ToBoolean(reader["Estado"])
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerTipoObjetivoPorId: " + ex.Message);
                    }
                }
            }
            return tipo;
        }
    }
}