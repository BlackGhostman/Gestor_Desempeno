using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Text; // Para StringBuilder

namespace Gestor_Desempeno
{

    // Clase para representar una Meta con datos relacionados
    public class MetaInfo
    {
        public int IdMeta { get; set; }
        public int? IdObjetivo { get; set; }
        public int? NumMeta { get; set; }
        public string Descripcion { get; set; }
        public int? Id_Detalle_Estado { get; set; }

        // Propiedades adicionales para mostrar/filtrar
        public string NombreObjetivo { get; set; }
        public int? IdTipoObjetivo { get; set; } // Para filtrar por tipo
        public string NombreTipoObjetivo { get; set; }
        public string DescripcionEstado { get; set; }
        public string DescripcionObjetivo { get; set; } // Nueva propiedad
        public string FichaTecnica { get; set; }
    }

    public class MetaDAL
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }

        // Método para obtener Metas con filtros opcionales
        public List<MetaInfo> ObtenerMetas(int? idTipoObjetivoFiltro = null, int? idObjetivoFiltro = null, int? numMetaFiltro = null, int? Id_Detalle_Estado = null)
        {
            List<MetaInfo> lista = new List<MetaInfo>();
            var parameters = new Dictionary<string, object>();

            // Construir la consulta base con JOINs
            StringBuilder queryBuilder = new StringBuilder(@"
            SELECT
                m.Id_Meta, m.Id_Objetivo, m.Num_Meta, m.Descripcion AS DescripcionMeta, m.Id_Detalle_Estado, m.Ficha_Tecnica,
                o.Nombre AS NombreObjetivo,
                o.Id_Tipo_Objetivo,
                t.Nombre AS NombreTipoObjetivo,
                d.Descripcion AS DescripcionEstado, o.Descripcion as DescripcionObjetivo
            FROM dbo.Meta m
            INNER JOIN dbo.Objetivo o ON m.Id_Objetivo = o.Id_Objetivo
            LEFT JOIN dbo.Tipo_Objetivo t ON o.Id_Tipo_Objetivo = t.Id_Tipo_Objetivo
            LEFT JOIN dbo.Detalle_Estado d ON m.Id_Detalle_Estado = d.Id_Detalle_Estado
        ");

            // Añadir condiciones WHERE dinámicamente
            StringBuilder whereClause = new StringBuilder();

            if (idTipoObjetivoFiltro.HasValue && idTipoObjetivoFiltro.Value > 0)
            {
                whereClause.Append(" AND o.Id_Tipo_Objetivo = @IdTipoObjetivoFiltro");
                parameters.Add("@IdTipoObjetivoFiltro", idTipoObjetivoFiltro.Value);
            }
            if (idObjetivoFiltro.HasValue && idObjetivoFiltro.Value > 0)
            {
                whereClause.Append(" AND m.Id_Objetivo = @IdObjetivoFiltro");
                parameters.Add("@IdObjetivoFiltro", idObjetivoFiltro.Value);
            }
            if (numMetaFiltro.HasValue) // Num_Meta puede ser 0 o cualquier número
            {
                whereClause.Append(" AND m.Num_Meta = @NumMetaFiltro");
                parameters.Add("@NumMetaFiltro", numMetaFiltro.Value);
            }
            if (Id_Detalle_Estado.HasValue) // Num_Meta puede ser 0 o cualquier número
            {
                whereClause.Append(" AND m.Id_Detalle_Estado = @Id_Detalle_Estado");
                parameters.Add("@Id_Detalle_Estado", Id_Detalle_Estado.Value);
            }

            // Añadir el WHERE si hay condiciones
            if (whereClause.Length > 0)
            {
                // Quitar el primer " AND " y añadir " WHERE "
                queryBuilder.Append(" WHERE ").Append(whereClause.ToString().Substring(5));
            }

            queryBuilder.Append(" ORDER BY o.Nombre, m.Num_Meta"); // Ordenar por objetivo, luego número de meta

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(queryBuilder.ToString(), con))
                {
                    // Añadir parámetros
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
                                lista.Add(new MetaInfo
                                {
                                    IdMeta = Convert.ToInt32(reader["Id_Meta"]),
                                    IdObjetivo = reader["Id_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Objetivo"]) : (int?)null,
                                    NumMeta = reader["Num_Meta"] != DBNull.Value ? Convert.ToInt32(reader["Num_Meta"]) : (int?)null,
                                    Descripcion = reader["DescripcionMeta"]?.ToString() ?? string.Empty,
                                    Id_Detalle_Estado = reader["Id_Detalle_Estado"] != DBNull.Value ? Convert.ToInt32(reader["Id_Detalle_Estado"]) : (int?)null,
                                    NombreObjetivo = reader["NombreObjetivo"]?.ToString() ?? "N/A",
                                    IdTipoObjetivo = reader["Id_Tipo_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Tipo_Objetivo"]) : (int?)null,
                                    NombreTipoObjetivo = reader["NombreTipoObjetivo"]?.ToString() ?? "N/A",
                                    DescripcionEstado = reader["DescripcionEstado"]?.ToString() ?? "N/A",
                                    DescripcionObjetivo = reader["DescripcionObjetivo"]?.ToString() ?? "N/A",
                                    FichaTecnica = reader["Ficha_Tecnica"]?.ToString() ?? string.Empty
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerMetas: " + ex.Message);
                        throw; // Re-throw
                    }
                }
            }
            return lista;
        }

        // Método para insertar una nueva meta
        public int InsertarMeta(int? idObjetivo, int? numMeta, string descripcion, int? Id_Detalle_Estado, string fichaTecnica)
        {
            int nuevoId = -1;
            // Validaciones básicas
            if (!idObjetivo.HasValue || idObjetivo.Value <= 0 || !Id_Detalle_Estado.HasValue || Id_Detalle_Estado.Value <= 0)
            {
                Console.WriteLine("Error InsertarMeta: Objetivo y Estado son requeridos.");
                return -1;
            }

            string query = @"INSERT INTO dbo.Meta (Id_Objetivo, Num_Meta, Descripcion, Id_Detalle_Estado, Ficha_Tecnica)
                         VALUES (@IdObjetivo, @NumMeta, @Descripcion, @Id_Detalle_Estado, @FichaTecnica);
                         SELECT SCOPE_IDENTITY();";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdObjetivo", idObjetivo.Value);
                    cmd.Parameters.AddWithValue("@NumMeta", numMeta.HasValue ? (object)numMeta.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : (object)descripcion);
                    cmd.Parameters.AddWithValue("@Id_Detalle_Estado", Id_Detalle_Estado.Value);
                    cmd.Parameters.AddWithValue("@FichaTecnica", string.IsNullOrWhiteSpace(fichaTecnica) ? DBNull.Value : (object)fichaTecnica);

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
                        Console.WriteLine("Error en InsertarMeta: " + ex.Message);
                        throw;
                    }
                }
            }
            return nuevoId;
        }

        // Método para actualizar una meta existente
        public bool ActualizarMeta(int idMeta, int? idObjetivo, int? numMeta, string descripcion, int? Id_Detalle_Estado, string fichaTecnica)
        {
            bool actualizado = false;
            // Validaciones básicas
            if (idMeta <= 0 || !idObjetivo.HasValue || idObjetivo.Value <= 0 || !Id_Detalle_Estado.HasValue || Id_Detalle_Estado.Value <= 0)
            {
                Console.WriteLine("Error ActualizarMeta: ID Meta, Objetivo y Estado son requeridos.");
                return false;
            }

            string query = @"UPDATE dbo.Meta SET
                            Id_Objetivo = @IdObjetivo,
                            Num_Meta = @NumMeta,
                            Descripcion = @Descripcion,
                            Id_Detalle_Estado = @Id_Detalle_Estado,
                            Ficha_Tecnica = @FichaTecnica
                         WHERE Id_Meta = @IdMeta";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMeta", idMeta);
                    cmd.Parameters.AddWithValue("@IdObjetivo", idObjetivo.Value);
                    cmd.Parameters.AddWithValue("@NumMeta", numMeta.HasValue ? (object)numMeta.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : (object)descripcion);
                    cmd.Parameters.AddWithValue("@Id_Detalle_Estado", Id_Detalle_Estado.Value);
                    cmd.Parameters.AddWithValue("@FichaTecnica", string.IsNullOrWhiteSpace(fichaTecnica) ? DBNull.Value : (object)fichaTecnica);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        actualizado = (rowsAffected > 0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ActualizarMeta: " + ex.Message);
                        throw;
                    }
                }
            }
            return actualizado;
        }

        // Método para eliminar lógicamente una meta (actualizar estado)
        public bool EliminarMetaLogico(int idMeta, int idEstadoInactivo)
        {
            if (idMeta <= 0 || idEstadoInactivo <= 0) return false;
            bool eliminado = false;
            string query = "UPDATE dbo.Meta SET Id_Detalle_Estado = @IdEstadoInactivo WHERE Id_Meta = @IdMeta";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdEstadoInactivo", idEstadoInactivo);
                    cmd.Parameters.AddWithValue("@IdMeta", idMeta);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        eliminado = (rowsAffected > 0);
                    }
                    catch (SqlException ex)
                    {
                        // Podría fallar si hay FKs en Meta_Departamental apuntando a esta Meta
                        if (ex.Number == 547)
                        {
                            Console.WriteLine($"Error al eliminar Meta (ID: {idMeta}): Existen Metas Departamentales asociadas.");
                            throw new InvalidOperationException("No se puede eliminar la meta porque tiene Metas Departamentales asociadas.", ex);
                        }
                        else { throw; }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en EliminarMetaLogico: " + ex.Message);
                        throw;
                    }
                }
            }
            return eliminado;
        }

        // Método para obtener una meta específica por ID
        public MetaInfo ObtenerMetaPorId(int idMeta)
        {
            MetaInfo meta = null;
            string query = @"SELECT
                            m.Id_Meta, m.Id_Objetivo, m.Num_Meta, m.Descripcion AS DescripcionMeta, m.Id_Detalle_Estado, m.Ficha_Tecnica,
                            o.Nombre AS NombreObjetivo,
                            o.Id_Tipo_Objetivo,
                            t.Nombre AS NombreTipoObjetivo,
                            d.Descripcion AS DescripcionEstado, o.Descripcion as DescripcionObjetivo
                         FROM dbo.Meta m
                         INNER JOIN dbo.Objetivo o ON m.Id_Objetivo = o.Id_Objetivo
                         LEFT JOIN dbo.Tipo_Objetivo t ON o.Id_Tipo_Objetivo = t.Id_Tipo_Objetivo
                         LEFT JOIN dbo.Detalle_Estado d ON m.Id_Detalle_Estado = d.Id_Detalle_Estado
                         WHERE m.Id_Meta = @IdMeta";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMeta", idMeta);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                meta = new MetaInfo
                                {
                                    IdMeta = Convert.ToInt32(reader["Id_Meta"]),
                                    IdObjetivo = reader["Id_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Objetivo"]) : (int?)null,
                                    NumMeta = reader["Num_Meta"] != DBNull.Value ? Convert.ToInt32(reader["Num_Meta"]) : (int?)null,
                                    Descripcion = reader["DescripcionMeta"]?.ToString() ?? string.Empty,
                                    Id_Detalle_Estado = reader["Id_Detalle_Estado"] != DBNull.Value ? Convert.ToInt32(reader["Id_Detalle_Estado"]) : (int?)null,
                                    NombreObjetivo = reader["NombreObjetivo"]?.ToString() ?? "N/A",
                                    IdTipoObjetivo = reader["Id_Tipo_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Tipo_Objetivo"]) : (int?)null,
                                    NombreTipoObjetivo = reader["NombreTipoObjetivo"]?.ToString() ?? "N/A",
                                    DescripcionEstado = reader["DescripcionEstado"]?.ToString() ?? "N/A",
                                    DescripcionObjetivo = reader["DescripcionObjetivo"]?.ToString() ?? "N/A",
                                    FichaTecnica = reader["Ficha_Tecnica"]?.ToString() ?? string.Empty
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerMetaPorId: " + ex.Message);
                    }
                }
            }
            return meta;
        }
    }

}