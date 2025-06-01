using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;

namespace Gestor_Desempeno
{


    public class MetaDepartamentalInfo
    {
        public int IdMetaDepartamental { get; set; }
        public int? IdMeta { get; set; }
        public int? Id_Area_Ejecutora { get; set; }
        public string Descripcion { get; set; } // Descripción de Meta Departamental
        public int? PesoPonderado { get; set; }
        public string Indicador { get; set; }
        public string Alcance { get; set; }
        public int? Prioridad { get; set; }
        public DateTime? FechaInicial { get; set; }
        public DateTime? FechaFinal { get; set; }
        public int? Id_Detalle_Estado { get; set; }

        // Propiedades adicionales
        public string DescripcionMetaPadre { get; set; } // Descripción de la tabla Meta
        public int? NumMetaPadre { get; set; } // Num_Meta de la tabla Meta
        public string NombreAreaEjecutora { get; set; }
        public string DescripcionEstado { get; set; }
        public int? IdTipoObjetivo { get; set; } // Para filtrar
        public string NombreTipoObjetivo { get; set; } // Para mostrar/filtrar

        // ** NEW: Property for Dropdown Display **
        public string DisplayTextForDropdown
        {
            get
            {
                // Combine relevant info for easy selection
                return $"{(NumMetaPadre.HasValue ? NumMetaPadre.Value + ". " : "")}{NombreAreaEjecutora ?? "Sin Área"} - {Descripcion?.Substring(0, Math.Min(Descripcion.Length, 50)) ?? "Sin Desc."}...";
            }
        }
    }


    public class MetaDepartamentalDAL
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }

        // ID de la Clase para los estados de Meta Departamental (AJUSTAR SI ES NECESARIO)
        private const int ID_CLASE_META_DEP = 3;

        // Método para obtener Metas Departamentales con filtros
        public List<MetaDepartamentalInfo> ObtenerMetasDepartamentales(string usuario, int? idTipoObjetivoFiltro = null, int? numMetaFiltro = null, int? idAreaEjecutoraFiltro = null) // Added Area filter
        {
            List<MetaDepartamentalInfo> lista = new List<MetaDepartamentalInfo>();
            var parameters = new Dictionary<string, object>();

            StringBuilder queryBuilder = new StringBuilder(@"
                SELECT
                    md.Id_Meta_Departamental, md.Id_Meta, md.Id_Area_Ejecutora, md.Descripcion,
                    md.Peso_Ponderado, md.Indicador, md.Alcance, md.Prioridad,
                    md.Fecha_Inicial, md.Fecha_Final, md.Id_Detalle_Estado,
                    m.Descripcion AS DescripcionMetaPadre,
                    m.Num_Meta AS NumMetaPadre,
                    ae.Nombre AS NombreAreaEjecutora,
                    de.Descripcion AS DescripcionEstado,
                    o.Id_Tipo_Objetivo,
                    t.Nombre AS NombreTipoObjetivo
                FROM dbo.Meta_Departamental md
                INNER JOIN dbo.Meta m ON md.Id_Meta = m.Id_Meta
                INNER JOIN dbo.Objetivo o ON m.Id_Objetivo = o.Id_Objetivo
                LEFT JOIN dbo.Tipo_Objetivo t ON o.Id_Tipo_Objetivo = t.Id_Tipo_Objetivo
                LEFT JOIN dbo.Area_Ejecutora ae ON md.Id_Area_Ejecutora = ae.Id_Area_Ejecutora
                LEFT JOIN dbo.Detalle_Estado de ON md.Id_Detalle_Estado = de.Id_Detalle_Estado
				inner join dbo.Encargado_Area ea On ea.Id_Area_Ejecutora = ae.Id_Area_Ejecutora
				where ea.Usuario = '" + usuario + "'");

            StringBuilder whereClause = new StringBuilder();

            // Apply filters
            if (idTipoObjetivoFiltro.HasValue && idTipoObjetivoFiltro.Value > 0)
            {
                whereClause.Append(" AND o.Id_Tipo_Objetivo = @IdTipoObjetivoFiltro");
                parameters.Add("@IdTipoObjetivoFiltro", idTipoObjetivoFiltro.Value);
            }
            if (numMetaFiltro.HasValue)
            {
                whereClause.Append(" AND m.Num_Meta = @NumMetaFiltro");
                parameters.Add("@NumMetaFiltro", numMetaFiltro.Value);
            }
            if (idAreaEjecutoraFiltro.HasValue && idAreaEjecutoraFiltro.Value > 0) // Added Area filter condition
            {
                whereClause.Append(" AND md.Id_Area_Ejecutora = @IdAreaEjecutoraFiltro");
                parameters.Add("@IdAreaEjecutoraFiltro", idAreaEjecutoraFiltro.Value);
            }
            // Optional: Filter by active state?
            // whereClause.Append(" AND de.Descripcion = 'Activo'"); // Or by ID if known

            if (whereClause.Length > 0)
            {
                queryBuilder.Append(" WHERE ").Append(whereClause.ToString().Substring(5)); // Remove leading " AND "
            }

            queryBuilder.Append(" ORDER BY ae.Nombre, m.Num_Meta"); // Orden sugerido

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(queryBuilder.ToString(), con))
                {
                    foreach (var param in parameters) { cmd.Parameters.AddWithValue(param.Key, param.Value); }

                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new MetaDepartamentalInfo
                                { /* ... mapping ... */
                                    IdMetaDepartamental = Convert.ToInt32(reader["Id_Meta_Departamental"]),
                                    IdMeta = reader["Id_Meta"] != DBNull.Value ? Convert.ToInt32(reader["Id_Meta"]) : (int?)null,
                                    Id_Area_Ejecutora = reader["Id_Area_Ejecutora"] != DBNull.Value ? Convert.ToInt32(reader["Id_Area_Ejecutora"]) : (int?)null,
                                    Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                                    PesoPonderado = reader["Peso_Ponderado"] != DBNull.Value ? Convert.ToInt32(reader["Peso_Ponderado"]) : (int?)null,
                                    Indicador = reader["Indicador"]?.ToString() ?? string.Empty,
                                    Alcance = reader["Alcance"]?.ToString() ?? string.Empty,
                                    Prioridad = reader["Prioridad"] != DBNull.Value ? Convert.ToInt32(reader["Prioridad"]) : (int?)null,
                                    FechaInicial = reader["Fecha_Inicial"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Inicial"]) : (DateTime?)null,
                                    FechaFinal = reader["Fecha_Final"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Final"]) : (DateTime?)null,
                                    Id_Detalle_Estado = reader["Id_Detalle_Estado"] != DBNull.Value ? Convert.ToInt32(reader["Id_Detalle_Estado"]) : (int?)null,
                                    DescripcionMetaPadre = reader["DescripcionMetaPadre"]?.ToString() ?? string.Empty,
                                    NumMetaPadre = reader["NumMetaPadre"] != DBNull.Value ? Convert.ToInt32(reader["NumMetaPadre"]) : (int?)null,
                                    NombreAreaEjecutora = reader["NombreAreaEjecutora"]?.ToString() ?? "N/A",
                                    DescripcionEstado = reader["DescripcionEstado"]?.ToString() ?? "N/A",
                                    IdTipoObjetivo = reader["Id_Tipo_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Tipo_Objetivo"]) : (int?)null,
                                    NombreTipoObjetivo = reader["NombreTipoObjetivo"]?.ToString() ?? "N/A"
                                });
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("Error en ObtenerMetasDepartamentales: " + ex.Message); throw; }
                }
            }
            return lista;
        }


        // Método para insertar una nueva meta departamental
        public int InsertarMetaDepartamental(int? idMeta, int? idAreaEjecutora, string descripcion, int? pesoPonderado, string indicador, string alcance, int? prioridad, DateTime? fechaInicial, DateTime? fechaFinal, int? idDetalleEstado)
        {
            int nuevoId = -1;
            if (!idMeta.HasValue || idMeta.Value <= 0 || !idAreaEjecutora.HasValue || idAreaEjecutora.Value <= 0 || !idDetalleEstado.HasValue || idDetalleEstado.Value <= 0) { return -1; }
            DateTime fechaInicialReal = fechaInicial ?? DateTime.Today;
            string query = @"INSERT INTO dbo.Meta_Departamental (Id_Meta, Id_Area_Ejecutora, Descripcion, Peso_Ponderado, Indicador, Alcance, Prioridad, Fecha_Inicial, Fecha_Final, Id_Detalle_Estado) VALUES (@IdMeta, @Id_Area_Ejecutora, @Descripcion, @PesoPonderado, @Indicador, @Alcance, @Prioridad, @FechaInicial, @FechaFinal, @IdDetalleEstado); SELECT SCOPE_IDENTITY();";
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMeta", idMeta.Value);
                    cmd.Parameters.AddWithValue("@Id_Area_Ejecutora", idAreaEjecutora.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : (object)descripcion);
                    cmd.Parameters.AddWithValue("@PesoPonderado", pesoPonderado.HasValue ? (object)pesoPonderado.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Indicador", string.IsNullOrWhiteSpace(indicador) ? DBNull.Value : (object)indicador);
                    cmd.Parameters.AddWithValue("@Alcance", string.IsNullOrWhiteSpace(alcance) ? DBNull.Value : (object)alcance);
                    cmd.Parameters.AddWithValue("@Prioridad", prioridad.HasValue ? (object)prioridad.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaInicial", fechaInicialReal);
                    cmd.Parameters.AddWithValue("@FechaFinal", fechaFinal.HasValue ? (object)fechaFinal.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdDetalleEstado", idDetalleEstado.Value);
                    try { con.Open(); object result = cmd.ExecuteScalar(); if (result != null && result != DBNull.Value) { nuevoId = Convert.ToInt32(result); } }
                    catch (Exception ex) { Console.WriteLine("Error en InsertarMetaDepartamental: " + ex.Message); throw; }
                }
            }
            return nuevoId;
        }

        // Método para actualizar una meta departamental existente
        public bool ActualizarMetaDepartamental(int idMetaDepartamental, int? idMeta, int? idAreaEjecutora, string descripcion, int? pesoPonderado, string indicador, string alcance, int? prioridad, DateTime? fechaInicial, DateTime? fechaFinal, int? idDetalleEstado)
        {
            bool actualizado = false;
            if (idMetaDepartamental <= 0 || !idMeta.HasValue || idMeta.Value <= 0 || !idAreaEjecutora.HasValue || idAreaEjecutora.Value <= 0 || !idDetalleEstado.HasValue || idDetalleEstado.Value <= 0) { return false; }
            DateTime fechaInicialReal = fechaInicial ?? DateTime.Today;
            string query = @"UPDATE dbo.Meta_Departamental SET Id_Meta = @IdMeta, Id_Area_Ejecutora = @Id_Area_Ejecutora, Descripcion = @Descripcion, Peso_Ponderado = @PesoPonderado, Indicador = @Indicador, Alcance = @Alcance, Prioridad = @Prioridad, Fecha_Inicial = @FechaInicial, Fecha_Final = @FechaFinal, Id_Detalle_Estado = @IdDetalleEstado WHERE Id_Meta_Departamental = @IdMetaDepartamental";
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMetaDepartamental", idMetaDepartamental);
                    cmd.Parameters.AddWithValue("@IdMeta", idMeta.Value);
                    cmd.Parameters.AddWithValue("@Id_Area_Ejecutora", idAreaEjecutora.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : (object)descripcion);
                    cmd.Parameters.AddWithValue("@PesoPonderado", pesoPonderado.HasValue ? (object)pesoPonderado.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Indicador", string.IsNullOrWhiteSpace(indicador) ? DBNull.Value : (object)indicador);
                    cmd.Parameters.AddWithValue("@Alcance", string.IsNullOrWhiteSpace(alcance) ? DBNull.Value : (object)alcance);
                    cmd.Parameters.AddWithValue("@Prioridad", prioridad.HasValue ? (object)prioridad.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaInicial", fechaInicialReal);
                    cmd.Parameters.AddWithValue("@FechaFinal", fechaFinal.HasValue ? (object)fechaFinal.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdDetalleEstado", idDetalleEstado.Value);
                    try { con.Open(); int rowsAffected = cmd.ExecuteNonQuery(); actualizado = (rowsAffected > 0); }
                    catch (Exception ex) { Console.WriteLine("Error en ActualizarMetaDepartamental: " + ex.Message); throw; }
                }
            }
            return actualizado;
        }

        // Método para eliminar lógicamente una meta departamental
        public bool EliminarMetaDepartamentalLogico(int idMetaDepartamental, int idEstadoInactivo)
        {
            if (idMetaDepartamental <= 0 || idEstadoInactivo <= 0) return false;
            bool eliminado = false;
            string query = "UPDATE dbo.Meta_Departamental SET Id_Detalle_Estado = @IdEstadoInactivo WHERE Id_Meta_Departamental = @IdMetaDepartamental";
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdEstadoInactivo", idEstadoInactivo);
                    cmd.Parameters.AddWithValue("@IdMetaDepartamental", idMetaDepartamental);
                    try { con.Open(); int rowsAffected = cmd.ExecuteNonQuery(); eliminado = (rowsAffected > 0); }
                    catch (SqlException ex) { if (ex.Number == 547) { throw new InvalidOperationException("No se puede eliminar la meta departamental porque tiene Metas Individuales asociadas.", ex); } else { throw; } }
                    catch (Exception ex) { Console.WriteLine("Error en EliminarMetaDepartamentalLogico: " + ex.Message); throw; }
                }
            }
            return eliminado;
        }

        // Método para obtener una meta departamental específica por ID
        public MetaDepartamentalInfo ObtenerMetaDepartamentalPorId(int idMetaDepartamental)
        {
            MetaDepartamentalInfo metaDep = null;
            string query = @"SELECT md.Id_Meta_Departamental, md.Id_Meta, md.Id_Area_Ejecutora, md.Descripcion, md.Peso_Ponderado, md.Indicador, md.Alcance, md.Prioridad, md.Fecha_Inicial, md.Fecha_Final, md.Id_Detalle_Estado, m.Descripcion AS DescripcionMetaPadre, m.Num_Meta AS NumMetaPadre, ae.Nombre AS NombreAreaEjecutora, de.Descripcion AS DescripcionEstado, o.Id_Tipo_Objetivo, t.Nombre AS NombreTipoObjetivo FROM dbo.Meta_Departamental md INNER JOIN dbo.Meta m ON md.Id_Meta = m.Id_Meta INNER JOIN dbo.Objetivo o ON m.Id_Objetivo = o.Id_Objetivo LEFT JOIN dbo.Tipo_Objetivo t ON o.Id_Tipo_Objetivo = t.Id_Tipo_Objetivo LEFT JOIN dbo.Area_Ejecutora ae ON md.Id_Area_Ejecutora = ae.Id_Area_Ejecutora LEFT JOIN dbo.Detalle_Estado de ON md.Id_Detalle_Estado = de.Id_Detalle_Estado WHERE md.Id_Meta_Departamental = @IdMetaDepartamental";
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMetaDepartamental", idMetaDepartamental);
                    try { con.Open(); using (SqlDataReader reader = cmd.ExecuteReader()) { if (reader.Read()) { metaDep = new MetaDepartamentalInfo { /* ... mapping ... */ IdMetaDepartamental = Convert.ToInt32(reader["Id_Meta_Departamental"]), IdMeta = reader["Id_Meta"] != DBNull.Value ? Convert.ToInt32(reader["Id_Meta"]) : (int?)null, Id_Area_Ejecutora = reader["Id_Area_Ejecutora"] != DBNull.Value ? Convert.ToInt32(reader["Id_Area_Ejecutora"]) : (int?)null, Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty, PesoPonderado = reader["Peso_Ponderado"] != DBNull.Value ? Convert.ToInt32(reader["Peso_Ponderado"]) : (int?)null, Indicador = reader["Indicador"]?.ToString() ?? string.Empty, Alcance = reader["Alcance"]?.ToString() ?? string.Empty, Prioridad = reader["Prioridad"] != DBNull.Value ? Convert.ToInt32(reader["Prioridad"]) : (int?)null, FechaInicial = reader["Fecha_Inicial"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Inicial"]) : (DateTime?)null, FechaFinal = reader["Fecha_Final"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Final"]) : (DateTime?)null, Id_Detalle_Estado = reader["Id_Detalle_Estado"] != DBNull.Value ? Convert.ToInt32(reader["Id_Detalle_Estado"]) : (int?)null, DescripcionMetaPadre = reader["DescripcionMetaPadre"]?.ToString() ?? string.Empty, NumMetaPadre = reader["NumMetaPadre"] != DBNull.Value ? Convert.ToInt32(reader["NumMetaPadre"]) : (int?)null, NombreAreaEjecutora = reader["NombreAreaEjecutora"]?.ToString() ?? "N/A", DescripcionEstado = reader["DescripcionEstado"]?.ToString() ?? "N/A", IdTipoObjetivo = reader["Id_Tipo_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Tipo_Objetivo"]) : (int?)null, NombreTipoObjetivo = reader["NombreTipoObjetivo"]?.ToString() ?? "N/A" }; } } }
                    catch (Exception ex) { Console.WriteLine("Error en ObtenerMetaDepartamentalPorId: " + ex.Message); }
                }
            }
            return metaDep;
        }
    }

}