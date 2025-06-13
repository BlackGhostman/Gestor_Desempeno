using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text; // Needed for StringBuilder

namespace Gestor_Desempeno
{
  

    // Clase para representar un Objetivo con datos relacionados
    public class ObjetivoInfo
    {
        public int Id_Objetivo { get; set; }
        public int? Id_Tipo_Objetivo { get; set; }
        public int? Id_Periodo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int? NumObjetivo { get; set; }
        public int? Id_Detalle_Estado { get; set; }

        // Propiedades adicionales para mostrar nombres
        public string NombreTipoObjetivo { get; set; }
        public string NombrePeriodo { get; set; }
        public string DescripcionEstado { get; set; }
    }

    public class ObjetivoDAL
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }

        // **UPDATED:** Método para obtener objetivos con filtro opcional por Tipo de Objetivo
        public List<ObjetivoInfo> ObtenerObjetivos(int Id_Detalle_Estado, int? idTipoObjetivoFiltro = null)
        {
            List<ObjetivoInfo> lista = new List<ObjetivoInfo>();

            // Query base
            string baseQuery = @"SELECT
                            o.Id_Objetivo, o.Id_Tipo_Objetivo, o.Id_Periodo, o.Nombre, o.Descripcion,
                            o.Num_Objetivo, o.Id_Detalle_Estado,
                            t.Nombre AS NombreTipoObjetivo,
                            p.Nombre AS NombrePeriodo,
                            d.Descripcion AS DescripcionEstado
                          FROM dbo.Objetivo o
                          LEFT JOIN dbo.Tipo_Objetivo t ON o.Id_Tipo_Objetivo = t.Id_Tipo_Objetivo
                          LEFT JOIN dbo.Periodo p ON o.Id_Periodo = p.Id_Periodo
                          LEFT JOIN dbo.Detalle_Estado d ON o.Id_Detalle_Estado = d.Id_Detalle_Estado";

            StringBuilder queryBuilder = new StringBuilder(baseQuery);
            var parameters = new Dictionary<string, object>();
            var whereClauses = new List<string>();

            // Construcción dinámica y segura de la cláusula WHERE
            if (idTipoObjetivoFiltro.HasValue && idTipoObjetivoFiltro.Value > 0)
            {
                whereClauses.Add("o.Id_Tipo_Objetivo = @IdTipoObjetivoFiltro");
                parameters["@IdTipoObjetivoFiltro"] = idTipoObjetivoFiltro.Value;
            }

            if (Id_Detalle_Estado > 0)
            {
                whereClauses.Add("d.Id_Detalle_Estado = @Id_Detalle_Estado");
                parameters["@Id_Detalle_Estado"] = Id_Detalle_Estado;
            }

            if (whereClauses.Any())
            {
                queryBuilder.Append(" WHERE ").Append(string.Join(" AND ", whereClauses));
            }

            queryBuilder.Append(" ORDER BY p.Nombre, o.Num_Objetivo, o.Nombre");

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(queryBuilder.ToString(), con))
                {
                    // Añadir todos los parámetros requeridos
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new ObjetivoInfo
                                {
                                    Id_Objetivo = Convert.ToInt32(reader["Id_Objetivo"]),
                                    Id_Tipo_Objetivo = reader["Id_Tipo_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Tipo_Objetivo"]) : (int?)null,
                                    Id_Periodo = reader["Id_Periodo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Periodo"]) : (int?)null,
                                    Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                                    Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                                    NumObjetivo = reader["Num_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Num_Objetivo"]) : (int?)null,
                                    Id_Detalle_Estado = reader["Id_Detalle_Estado"] != DBNull.Value ? Convert.ToInt32(reader["Id_Detalle_Estado"]) : (int?)null,
                                    NombreTipoObjetivo = reader["NombreTipoObjetivo"]?.ToString() ?? "N/A",
                                    NombrePeriodo = reader["NombrePeriodo"]?.ToString() ?? "N/A",
                                    DescripcionEstado = reader["DescripcionEstado"]?.ToString() ?? "N/A"
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerObjetivos: " + ex.Message);
                        throw; // Re-lanzar la excepción para que sea gestionada en un nivel superior
                    }
                }
            }
            return lista;
        }

        // Método para insertar un nuevo objetivo
        public int InsertarObjetivo(int? Id_Tipo_Objetivo, int? Id_Periodo, string nombre, string descripcion, int? numObjetivo, int? Id_Detalle_Estado)
        {
            int nuevoId = -1;
            string query = @"INSERT INTO dbo.Objetivo
                            (Id_Tipo_Objetivo, Id_Periodo, Nombre, Descripcion, Num_Objetivo, Id_Detalle_Estado)
                         VALUES
                            (@Id_Tipo_Objetivo, @Id_Periodo, @Nombre, @Descripcion, @NumObjetivo, @Id_Detalle_Estado);
                         SELECT SCOPE_IDENTITY();";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id_Tipo_Objetivo", Id_Tipo_Objetivo.HasValue ? (object)Id_Tipo_Objetivo.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_Periodo", Id_Periodo.HasValue ? (object)Id_Periodo.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Nombre", string.IsNullOrWhiteSpace(nombre) ? DBNull.Value : (object)nombre);
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : (object)descripcion);
                    cmd.Parameters.AddWithValue("@NumObjetivo", numObjetivo.HasValue ? (object)numObjetivo.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_Detalle_Estado", Id_Detalle_Estado.HasValue ? (object)Id_Detalle_Estado.Value : DBNull.Value);

                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            nuevoId = Convert.ToInt32(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en InsertarObjetivo: " + ex.Message);
                        throw; // Re-throw exception
                    }
                }
            }
            return nuevoId;
        }

        // Método para actualizar un objetivo existente
        public bool ActualizarObjetivo(int Id_Objetivo, int? Id_Tipo_Objetivo, int? Id_Periodo, string nombre, string descripcion, int? numObjetivo, int? Id_Detalle_Estado)
        {
            bool actualizado = false;
            string query = @"UPDATE dbo.Objetivo SET
                            Id_Tipo_Objetivo = @Id_Tipo_Objetivo,
                            Id_Periodo = @Id_Periodo,
                            Nombre = @Nombre,
                            Descripcion = @Descripcion,
                            Num_Objetivo = @NumObjetivo,
                            Id_Detalle_Estado = @Id_Detalle_Estado
                         WHERE Id_Objetivo = @Id_Objetivo";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id_Objetivo", Id_Objetivo);
                    cmd.Parameters.AddWithValue("@Id_Tipo_Objetivo", Id_Tipo_Objetivo.HasValue ? (object)Id_Tipo_Objetivo.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_Periodo", Id_Periodo.HasValue ? (object)Id_Periodo.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Nombre", string.IsNullOrWhiteSpace(nombre) ? DBNull.Value : (object)nombre);
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : (object)descripcion);
                    cmd.Parameters.AddWithValue("@NumObjetivo", numObjetivo.HasValue ? (object)numObjetivo.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_Detalle_Estado", Id_Detalle_Estado.HasValue ? (object)Id_Detalle_Estado.Value : DBNull.Value);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        actualizado = (rowsAffected > 0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ActualizarObjetivo: " + ex.Message);
                        throw; // Re-throw exception
                    }
                }
            }
            return actualizado;
        }

        // Método para eliminar lógicamente un objetivo (actualizar estado)
        public bool EliminarObjetivoLogico(int Id_Objetivo, int idEstadoInactivo)
        {
            if (idEstadoInactivo <= 0)
            {
                Console.WriteLine("Error en EliminarObjetivoLogico: ID de estado inactivo inválido.");
                return false; // No se puede eliminar sin un estado válido
            }
            bool eliminado = false;
            string query = "UPDATE dbo.Objetivo SET Id_Detalle_Estado = @IdEstadoInactivo WHERE Id_Objetivo = @Id_Objetivo";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdEstadoInactivo", idEstadoInactivo);
                    cmd.Parameters.AddWithValue("@Id_Objetivo", Id_Objetivo);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        eliminado = (rowsAffected > 0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en EliminarObjetivoLogico: " + ex.Message);
                        throw; // Re-throw exception
                    }
                }
            }
            return eliminado;
        }

        // Método para obtener un objetivo específico (puede ser útil para edición)
        public ObjetivoInfo ObtenerObjetivoPorId(int Id_Objetivo)
        {
            ObjetivoInfo objetivo = null;
            // Query similar a ObtenerObjetivos pero filtrando por ID
            string query = @"SELECT
                            o.Id_Objetivo, o.Id_Tipo_Objetivo, o.Id_Periodo, o.Nombre, o.Descripcion,
                            o.Num_Objetivo, o.Id_Detalle_Estado,
                            t.Nombre AS NombreTipoObjetivo,
                            p.Nombre AS NombrePeriodo,
                            d.Descripcion AS DescripcionEstado
                         FROM dbo.Objetivo o
                         LEFT JOIN dbo.Tipo_Objetivo t ON o.Id_Tipo_Objetivo = t.Id_Tipo_Objetivo
                         LEFT JOIN dbo.Periodo p ON o.Id_Periodo = p.Id_Periodo
                         LEFT JOIN dbo.Detalle_Estado d ON o.Id_Detalle_Estado = d.Id_Detalle_Estado
                         WHERE o.Id_Objetivo = @Id_Objetivo";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id_Objetivo", Id_Objetivo);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                objetivo = new ObjetivoInfo
                                {
                                    Id_Objetivo = Convert.ToInt32(reader["Id_Objetivo"]),
                                    Id_Tipo_Objetivo = reader["Id_Tipo_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Tipo_Objetivo"]) : (int?)null,
                                    Id_Periodo = reader["Id_Periodo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Periodo"]) : (int?)null,
                                    Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                                    Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                                    NumObjetivo = reader["Num_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Num_Objetivo"]) : (int?)null,
                                    Id_Detalle_Estado = reader["Id_Detalle_Estado"] != DBNull.Value ? Convert.ToInt32(reader["Id_Detalle_Estado"]) : (int?)null,
                                    NombreTipoObjetivo = reader["NombreTipoObjetivo"]?.ToString() ?? "N/A",
                                    NombrePeriodo = reader["NombrePeriodo"]?.ToString() ?? "N/A",
                                    DescripcionEstado = reader["DescripcionEstado"]?.ToString() ?? "N/A"
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerObjetivoPorId: " + ex.Message);
                    }
                }
            }
            return objetivo;
        }

    }


}