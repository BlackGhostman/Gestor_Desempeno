using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Globalization;


namespace Gestor_Desempeno
{
    public class MetaIndividualInfo
    {
        public int IdMetaIndividual { get; set; }
        public int? IdMetaDepartamental { get; set; }
        public string Usuario { get; set; }
        public string Descripcion { get; set; }
        public string Alcance { get; set; }
        public int? PesoPonderadoN4 { get; set; }
        public int? PesoPonderadoN5 { get; set; }
        public DateTime? FechaInicial { get; set; }
        public DateTime? FechaFinal { get; set; }
        public bool? EsFinalizable { get; set; }
        public int? IdDetalleEstado { get; set; }

        // Propiedades adicionales para mostrar/filtrar
        public string DescripcionMetaDepartamental { get; set; }
        public string NombreAreaEjecutora { get; set; }
        public int? NumMetaPadre { get; set; } // Num_Meta from Meta table
        public string NombreTipoObjetivo { get; set; } // From Tipo_Objetivo table
        public string DescripcionEstado { get; set; }
        public int? IdAreaEjecutora { get; set; } // Para filtro
        public int? IdTipoObjetivo { get; set; } // Para filtro
        public int? NumObjetivoPadre { get; set; } // Num_Objetivo from Objetivo table
        public string NombreObjetivoPadre { get; set; } // Nombre from Objetivo table

        // Property for combined display text (Lista 2)
        public string DisplayTextLista2
        {
            get
            {
                // Format: "NumObj.NumMeta Descripcion..."
                string numObjStr = NumObjetivoPadre.HasValue ? NumObjetivoPadre.Value.ToString() : "?";
                string numMetaStr = NumMetaPadre.HasValue ? NumMetaPadre.Value.ToString() : "?";
                string descShort = Descripcion?.Substring(0, Math.Min(Descripcion.Length, 60)) ?? "";
                return $"{numObjStr}.{numMetaStr} {descShort}{(Descripcion?.Length > 60 ? "..." : "")}";
            }
        }
    }

    public class MetaIndividualInfoViewModel
    {
        public MetaIndividualInfo Meta { get; set; }
        public string EstadoColorCss { get; set; }
        public string MensajeTiempo { get; set; }
        public string BadgeStyle { get; set; } // Para el estilo del badge (ej. color de fondo)

        public MetaIndividualInfoViewModel(MetaIndividualInfo meta)
        {
            this.Meta = meta ?? throw new ArgumentNullException(nameof(meta)); // Es bueno validar la entrada

            // Valores por defecto, se sobrescribirán en LoadMetasUsuario
            this.EstadoColorCss = "meta-semanal-original"; // Estilo por defecto para metas semanales no finalizables
            this.MensajeTiempo = "";
            this.BadgeStyle = "background-color: #0EA5E9; color: white;"; // Azul por defecto para metas semanales
        }
    }


    public class MetaIndividualDAL
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }

        // Dentro de tu clase MetaIndividualDAL en MetaIndividualDAL.cs

        public List<MetaIndividualInfo> ObtenerMetasIndividualesPorUsuariosYEstado(List<string> usuarios, int idDetalleEstado)
        {
            List<MetaIndividualInfo> lista = new List<MetaIndividualInfo>();
            if (usuarios == null || !usuarios.Any())
            {
                return lista;
            }

            var userParameters = new List<string>();
            var sqlParameters = new List<SqlParameter>();
            for (int i = 0; i < usuarios.Count; i++)
            {
                string paramName = $"@Usuario{i}";
                userParameters.Add(paramName);
                sqlParameters.Add(new SqlParameter(paramName, usuarios[i]));
            }
            string userInClause = string.Join(",", userParameters);

            string query = $@"
            SELECT
                mi.Id_Meta_Individual, mi.Id_Meta_Departamental, mi.Usuario, mi.Descripcion, mi.Alcance,
                mi.Peso_Ponderado_N4, mi.Peso_Ponderado_N5, mi.Fecha_Inicial, mi.Fecha_Final,
                mi.Es_Finalizable, mi.Id_Detalle_Estado,
                md.Descripcion AS DescripcionMetaDepartamental,
                ae.Id_Area_Ejecutora,
                ae.Nombre AS NombreAreaEjecutora,
                m.Num_Meta AS NumMetaPadre,
                o.Num_Objetivo AS NumObjetivoPadre,
                o.Nombre AS NombreObjetivoPadre,
                t.Id_Tipo_Objetivo,
                t.Nombre AS NombreTipoObjetivo,
                de.Descripcion AS DescripcionEstado
            FROM dbo.Meta_Individual mi
            INNER JOIN dbo.Meta_Departamental md ON mi.Id_Meta_Departamental = md.Id_Meta_Departamental
            INNER JOIN dbo.Meta m ON md.Id_Meta = m.Id_Meta
            INNER JOIN dbo.Objetivo o ON m.Id_Objetivo = o.Id_Objetivo
            LEFT JOIN dbo.Tipo_Objetivo t ON o.Id_Tipo_Objetivo = t.Id_Tipo_Objetivo
            LEFT JOIN dbo.Area_Ejecutora ae ON md.Id_Area_Ejecutora = ae.Id_Area_Ejecutora
            LEFT JOIN dbo.Detalle_Estado de ON mi.Id_Detalle_Estado = de.Id_Detalle_Estado
            WHERE mi.Usuario IN ({userInClause}) AND mi.Id_Detalle_Estado = @IdDetalleEstado
            ORDER BY mi.Usuario, mi.Fecha_Final DESC, m.Num_Meta";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddRange(sqlParameters.ToArray());
                    cmd.Parameters.AddWithValue("@IdDetalleEstado", idDetalleEstado);

                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new MetaIndividualInfo
                                {
                                    IdMetaIndividual = Convert.ToInt32(reader["Id_Meta_Individual"]),
                                    IdMetaDepartamental = reader["Id_Meta_Departamental"] != DBNull.Value ? Convert.ToInt32(reader["Id_Meta_Departamental"]) : (int?)null,
                                    Usuario = reader["Usuario"]?.ToString() ?? string.Empty,
                                    Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                                    Alcance = reader["Alcance"]?.ToString() ?? string.Empty,
                                    PesoPonderadoN4 = reader["Peso_Ponderado_N4"] != DBNull.Value ? Convert.ToInt32(reader["Peso_Ponderado_N4"]) : (int?)null,
                                    PesoPonderadoN5 = reader["Peso_Ponderado_N5"] != DBNull.Value ? Convert.ToInt32(reader["Peso_Ponderado_N5"]) : (int?)null,
                                    FechaInicial = reader["Fecha_Inicial"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Inicial"]) : (DateTime?)null,
                                    FechaFinal = reader["Fecha_Final"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Final"]) : (DateTime?)null,
                                    EsFinalizable = reader["Es_Finalizable"] != DBNull.Value ? Convert.ToBoolean(reader["Es_Finalizable"]) : (bool?)null,
                                    IdDetalleEstado = reader["Id_Detalle_Estado"] != DBNull.Value ? Convert.ToInt32(reader["Id_Detalle_Estado"]) : (int?)null,
                                    DescripcionMetaDepartamental = reader["DescripcionMetaDepartamental"]?.ToString() ?? "N/A",
                                    IdAreaEjecutora = reader["Id_Area_Ejecutora"] != DBNull.Value ? Convert.ToInt32(reader["Id_Area_Ejecutora"]) : (int?)null,
                                    NombreAreaEjecutora = reader["NombreAreaEjecutora"]?.ToString() ?? "N/A",
                                    NumMetaPadre = reader["NumMetaPadre"] != DBNull.Value ? Convert.ToInt32(reader["NumMetaPadre"]) : (int?)null,
                                    NumObjetivoPadre = reader["NumObjetivoPadre"] != DBNull.Value ? Convert.ToInt32(reader["NumObjetivoPadre"]) : (int?)null,
                                    NombreObjetivoPadre = reader["NombreObjetivoPadre"]?.ToString() ?? "N/A",
                                    IdTipoObjetivo = reader["Id_Tipo_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Tipo_Objetivo"]) : (int?)null,
                                    NombreTipoObjetivo = reader["NombreTipoObjetivo"]?.ToString() ?? "N/A",
                                    DescripcionEstado = reader["DescripcionEstado"]?.ToString() ?? "N/A"
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en ObtenerMetasIndividualesPorUsuariosYEstado: {ex.Message}");
                        throw;
                    }
                }
            }
            return lista;
        }

        public bool ActualizarEstadoMetaIndividual(int idMetaIndividual, int nuevoIdDetalleEstado)
        {
            string query = @"UPDATE dbo.Meta_Individual 
                     SET Id_Detalle_Estado = @NuevoIdDetalleEstado 
                     WHERE Id_Meta_Individual = @IdMetaIndividual";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@NuevoIdDetalleEstado", nuevoIdDetalleEstado);
                    cmd.Parameters.AddWithValue("@IdMetaIndividual", idMetaIndividual);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0; // Retorna true si al menos una fila fue actualizada
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en MetaIndividualDAL.ActualizarEstadoMetaIndividual: {ex.Message}");
                        // Considera registrar el error de forma más robusta si es necesario
                        return false;
                    }
                }
            }
        }

        // ID de la Clase para los estados de Meta Individual (AJUSTAR SI ES NECESARIO)
        private const int ID_CLASE_META_IND = 4;

        // ** ESTE ES EL MÉTODO QUE DEBE EXISTIR Y SER LLAMADO **
        // Método para obtener Metas Individuales con filtros


        public List<MetaIndividualInfo> ObtenerMetasIndividuales(
        int? idTipoObjetivoFiltro = null,
        int? numMetaFiltro = null,
        int? idAreaEjecutoraFiltro = null,
        string usuarioFiltro = null,
        string usuariosFiltro = null) // Ejemplo de entrada: "'rmayorga','aacuna','jmanzanares'"
        {
            List<MetaIndividualInfo> lista = new List<MetaIndividualInfo>();
            var parameters = new Dictionary<string, object>();

            StringBuilder queryBuilder = new StringBuilder(@"
            SELECT
                mi.Id_Meta_Individual, mi.Id_Meta_Departamental, mi.Usuario, mi.Descripcion, mi.Alcance,
                mi.Peso_Ponderado_N4, mi.Peso_Ponderado_N5, mi.Fecha_Inicial, mi.Fecha_Final,
                mi.Es_Finalizable, mi.Id_Detalle_Estado,
                md.Descripcion AS DescripcionMetaDepartamental,
                ae.Id_Area_Ejecutora,
                ae.Nombre AS NombreAreaEjecutora,
                m.Num_Meta AS NumMetaPadre,
                o.Num_Objetivo AS NumObjetivoPadre,
                o.Nombre AS NombreObjetivoPadre,
                t.Id_Tipo_Objetivo,
                t.Nombre AS NombreTipoObjetivo,
                de.Descripcion AS DescripcionEstado
            FROM dbo.Meta_Individual mi
            INNER JOIN dbo.Meta_Departamental md ON mi.Id_Meta_Departamental = md.Id_Meta_Departamental
            INNER JOIN dbo.Meta m ON md.Id_Meta = m.Id_Meta
            INNER JOIN dbo.Objetivo o ON m.Id_Objetivo = o.Id_Objetivo
            LEFT JOIN dbo.Tipo_Objetivo t ON o.Id_Tipo_Objetivo = t.Id_Tipo_Objetivo
            LEFT JOIN dbo.Area_Ejecutora ae ON md.Id_Area_Ejecutora = ae.Id_Area_Ejecutora
            LEFT JOIN dbo.Detalle_Estado de ON mi.Id_Detalle_Estado = de.Id_Detalle_Estado
        ");

            StringBuilder whereClause = new StringBuilder();

            // Aplicar filtros
            if (idTipoObjetivoFiltro.HasValue && idTipoObjetivoFiltro.Value > 0)
            {
                whereClause.Append(" AND t.Id_Tipo_Objetivo = @IdTipoObjetivoFiltro");
                parameters.Add("@IdTipoObjetivoFiltro", idTipoObjetivoFiltro.Value);
            }
            if (numMetaFiltro.HasValue)
            {
                whereClause.Append(" AND m.Num_Meta = @NumMetaFiltro");
                parameters.Add("@NumMetaFiltro", numMetaFiltro.Value);
            }
            if (idAreaEjecutoraFiltro.HasValue && idAreaEjecutoraFiltro.Value > 0)
            {
                whereClause.Append(" AND md.Id_Area_Ejecutora = @IdAreaEjecutoraFiltro");
                parameters.Add("@IdAreaEjecutoraFiltro", idAreaEjecutoraFiltro.Value);
            }
            if (!string.IsNullOrWhiteSpace(usuarioFiltro))
            {
                whereClause.Append(" AND mi.Usuario = @UsuarioFiltro");
                parameters.Add("@UsuarioFiltro", usuarioFiltro.Trim());
            }

            // ----- SECCIÓN CORREGIDA PARA usuariosFiltro (CON ELIMINACIÓN DE COMILLAS SIMPLES) -----
            if (!string.IsNullOrWhiteSpace(usuariosFiltro))
            {
                string[] usuarios = usuariosFiltro.Trim() // Trim general de la cadena completa
                                                 .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(u => u.Trim().Trim('\'')) // Trim de espacios Y LUEGO trim de comillas simples (') de cada parte
                                                 .Where(u => !string.IsNullOrWhiteSpace(u)) // Filtra elementos vacíos que pudieron quedar (si un usuario era solo "''" o ",' ',")
                                                 .ToArray();

                if (usuarios.Length > 0)
                {
                    List<string> paramNombres = new List<string>();
                    for (int i = 0; i < usuarios.Length; i++)
                    {
                        // 'usuarios[i]' ahora contendrá el nombre de usuario sin las comillas simples externas.
                        // Ejemplo: "rmayorga" en lugar de "'rmayorga'"
                        string paramNombre = $"@UsuariosFiltroParam{i}";
                        paramNombres.Add(paramNombre);
                        parameters.Add(paramNombre, usuarios[i]); // Se añade el nombre de usuario limpio
                    }
                    whereClause.Append($" AND mi.Usuario IN ({string.Join(",", paramNombres)})");
                }
            }
            // ----- FIN DE LA SECCIÓN CORREGIDA -----

            if (whereClause.Length > 0)
            {
                queryBuilder.Append(" WHERE ").Append(whereClause.ToString().Substring(5)); // Remueve el primer " AND "
            }

            queryBuilder.Append(" ORDER BY mi.Usuario, m.Num_Meta");

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(queryBuilder.ToString(), con))
                {
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
                                lista.Add(new MetaIndividualInfo
                                {
                                    IdMetaIndividual = Convert.ToInt32(reader["Id_Meta_Individual"]),
                                    IdMetaDepartamental = reader["Id_Meta_Departamental"] != DBNull.Value ? Convert.ToInt32(reader["Id_Meta_Departamental"]) : (int?)null,
                                    Usuario = reader["Usuario"]?.ToString() ?? string.Empty,
                                    Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                                    Alcance = reader["Alcance"]?.ToString() ?? string.Empty,
                                    PesoPonderadoN4 = reader["Peso_Ponderado_N4"] != DBNull.Value ? Convert.ToInt32(reader["Peso_Ponderado_N4"]) : (int?)null,
                                    PesoPonderadoN5 = reader["Peso_Ponderado_N5"] != DBNull.Value ? Convert.ToInt32(reader["Peso_Ponderado_N5"]) : (int?)null,
                                    FechaInicial = reader["Fecha_Inicial"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Inicial"]) : (DateTime?)null,
                                    FechaFinal = reader["Fecha_Final"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Final"]) : (DateTime?)null,
                                    EsFinalizable = reader["Es_Finalizable"] != DBNull.Value ? Convert.ToBoolean(reader["Es_Finalizable"]) : (bool?)null,
                                    IdDetalleEstado = reader["Id_Detalle_Estado"] != DBNull.Value ? Convert.ToInt32(reader["Id_Detalle_Estado"]) : (int?)null,
                                    DescripcionMetaDepartamental = reader["DescripcionMetaDepartamental"]?.ToString() ?? "N/A",
                                    IdAreaEjecutora = reader["Id_Area_Ejecutora"] != DBNull.Value ? Convert.ToInt32(reader["Id_Area_Ejecutora"]) : (int?)null,
                                    NombreAreaEjecutora = reader["NombreAreaEjecutora"]?.ToString() ?? "N/A",
                                    NumMetaPadre = reader["NumMetaPadre"] != DBNull.Value ? Convert.ToInt32(reader["NumMetaPadre"]) : (int?)null,
                                    NumObjetivoPadre = reader["NumObjetivoPadre"] != DBNull.Value ? Convert.ToInt32(reader["NumObjetivoPadre"]) : (int?)null,
                                    NombreObjetivoPadre = reader["NombreObjetivoPadre"]?.ToString() ?? "N/A",
                                    IdTipoObjetivo = reader["Id_Tipo_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Tipo_Objetivo"]) : (int?)null,
                                    NombreTipoObjetivo = reader["NombreTipoObjetivo"]?.ToString() ?? "N/A",
                                    DescripcionEstado = reader["DescripcionEstado"]?.ToString() ?? "N/A"
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerMetasIndividuales: " + ex.Message);
                        // Considera un logging más robusto o una gestión de excepciones específica para tu aplicación
                        throw;
                    }
                }
            }
            return lista;
        }

        // Method to get metas for a specific user (used by Desempeno page)
        public List<MetaIndividualInfo> ObtenerMetasIndividualesPorUsuario(string usuario)
        {
            // This method now simply calls the main filtering method with only the user filter applied
            return ObtenerMetasIndividuales(null, null, null, usuario,null);
        }

        // Método para "Metas Rápidas" que se alinea con la tabla [Meta_Individual].
        public static void InsertarMetaRapidaIndividual(
            int idMetaDepartamental,
            string usuario,
            string descripcion,
            DateTime fechaInicial,
            DateTime fechaFinal,
            bool esFinalizable,
            int idDetalleEstado,
            string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_InsertarMetaRapidaIndividual", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Meta_Departamental", idMetaDepartamental);
                    cmd.Parameters.AddWithValue("@Usuario", usuario);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                    cmd.Parameters.AddWithValue("@Fecha_Inicial", fechaInicial);
                    cmd.Parameters.AddWithValue("@Fecha_Final", fechaFinal);
                    cmd.Parameters.AddWithValue("@Es_Finalizable", !esFinalizable);
                    cmd.Parameters.AddWithValue("@Id_Detalle_Estado", idDetalleEstado);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }


        // Método para insertar una nueva meta individual
        public int InsertarMetaIndividual(int? idMetaDepartamental, string usuario, string descripcion, string alcance, int? pesoN4, int? pesoN5, DateTime? fechaInicial, DateTime? fechaFinal, bool? esFinalizable, int? idDetalleEstado)
        {
            int nuevoId = -1;
            if (!idMetaDepartamental.HasValue || idMetaDepartamental.Value <= 0 || string.IsNullOrWhiteSpace(usuario) || !idDetalleEstado.HasValue || idDetalleEstado.Value <= 0) { return -1; }
            DateTime fechaInicialReal = fechaInicial ?? DateTime.Today;
            DateTime? fechaFinalReal = (esFinalizable == true && !fechaFinal.HasValue) ? (DateTime?)null : fechaFinal;
            string query = @"INSERT INTO dbo.Meta_Individual (Id_Meta_Departamental, Usuario, Descripcion, Alcance, Peso_Ponderado_N4, Peso_Ponderado_N5, Fecha_Inicial, Fecha_Final, Es_Finalizable, Id_Detalle_Estado) VALUES (@IdMetaDepartamental, @Usuario, @Descripcion, @Alcance, @PesoN4, @PesoN5, @FechaInicial, @FechaFinal, @EsFinalizable, @IdDetalleEstado); SELECT SCOPE_IDENTITY();";
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMetaDepartamental", idMetaDepartamental.Value);
                    cmd.Parameters.AddWithValue("@Usuario", usuario.Trim());
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : (object)descripcion);
                    cmd.Parameters.AddWithValue("@Alcance", string.IsNullOrWhiteSpace(alcance) ? DBNull.Value : (object)alcance);
                    cmd.Parameters.AddWithValue("@PesoN4", pesoN4.HasValue ? (object)pesoN4.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@PesoN5", pesoN5.HasValue ? (object)pesoN5.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaInicial", fechaInicialReal);
                    cmd.Parameters.AddWithValue("@FechaFinal", fechaFinalReal.HasValue ? (object)fechaFinalReal.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@EsFinalizable", esFinalizable.HasValue ? (object)esFinalizable.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdDetalleEstado", idDetalleEstado.Value);
                    try { con.Open(); object result = cmd.ExecuteScalar(); if (result != null && result != DBNull.Value) { nuevoId = Convert.ToInt32(result); } }
                    catch (Exception ex) { Console.WriteLine("Error en InsertarMetaIndividual: " + ex.Message); throw; }
                }
            }
            return nuevoId;
        }

        // Método para actualizar una meta individual existente
        public bool ActualizarMetaIndividual(int idMetaIndividual, int? idMetaDepartamental, string usuario, string descripcion, string alcance, int? pesoN4, int? pesoN5, DateTime? fechaInicial, DateTime? fechaFinal, bool? esFinalizable, int? idDetalleEstado)
        {
            bool actualizado = false;
            if (idMetaIndividual <= 0 || !idMetaDepartamental.HasValue || idMetaDepartamental.Value <= 0 || string.IsNullOrWhiteSpace(usuario) || !idDetalleEstado.HasValue || idDetalleEstado.Value <= 0) { return false; }
            DateTime fechaInicialReal = fechaInicial ?? DateTime.Today;
            DateTime? fechaFinalReal = (esFinalizable == true && !fechaFinal.HasValue) ? (DateTime?)null : fechaFinal;
            string query = @"UPDATE dbo.Meta_Individual SET Id_Meta_Departamental = @IdMetaDepartamental, Usuario = @Usuario, Descripcion = @Descripcion, Alcance = @Alcance, Peso_Ponderado_N4 = @PesoN4, Peso_Ponderado_N5 = @PesoN5, Fecha_Inicial = @FechaInicial, Fecha_Final = @FechaFinal, Es_Finalizable = @EsFinalizable, Id_Detalle_Estado = @IdDetalleEstado WHERE Id_Meta_Individual = @IdMetaIndividual";
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMetaIndividual", idMetaIndividual);
                    cmd.Parameters.AddWithValue("@IdMetaDepartamental", idMetaDepartamental.Value);
                    cmd.Parameters.AddWithValue("@Usuario", usuario.Trim());
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : (object)descripcion);
                    cmd.Parameters.AddWithValue("@Alcance", string.IsNullOrWhiteSpace(alcance) ? DBNull.Value : (object)alcance);
                    cmd.Parameters.AddWithValue("@PesoN4", pesoN4.HasValue ? (object)pesoN4.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@PesoN5", pesoN5.HasValue ? (object)pesoN5.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaInicial", fechaInicialReal);
                    cmd.Parameters.AddWithValue("@FechaFinal", fechaFinalReal.HasValue ? (object)fechaFinalReal.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@EsFinalizable", esFinalizable.HasValue ? (object)esFinalizable.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdDetalleEstado", idDetalleEstado.Value);
                    try { con.Open(); int rowsAffected = cmd.ExecuteNonQuery(); actualizado = (rowsAffected > 0); }
                    catch (Exception ex) { Console.WriteLine("Error en ActualizarMetaIndividual: " + ex.Message); throw; }
                }
            }
            return actualizado;
        }

        // Método para eliminar lógicamente una meta individual
        public bool EliminarMetaIndividualLogico(int idMetaIndividual, int idEstadoInactivo)
        {
            if (idMetaIndividual <= 0 || idEstadoInactivo <= 0) return false;
            bool eliminado = false;
            string query = "UPDATE dbo.Meta_Individual SET Id_Detalle_Estado = @IdEstadoInactivo WHERE Id_Meta_Individual = @IdMetaIndividual";
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdEstadoInactivo", idEstadoInactivo);
                    cmd.Parameters.AddWithValue("@IdMetaIndividual", idMetaIndividual);
                    try { con.Open(); int rowsAffected = cmd.ExecuteNonQuery(); eliminado = (rowsAffected > 0); }
                    catch (SqlException ex) { if (ex.Number == 547) { throw new InvalidOperationException("No se puede eliminar la meta individual porque tiene Respuestas asociadas.", ex); } else { throw; } }
                    catch (Exception ex) { Console.WriteLine("Error en EliminarMetaIndividualLogico: " + ex.Message); throw; }
                }
            }
            return eliminado;
        }

        // Método para obtener una meta individual específica por ID
        public MetaIndividualInfo ObtenerMetaIndividualPorId(int idMetaIndividual)
        {
            MetaIndividualInfo metaInd = null;
            string query = @"SELECT mi.Id_Meta_Individual, mi.Id_Meta_Departamental, mi.Usuario, mi.Descripcion, mi.Alcance, mi.Peso_Ponderado_N4, mi.Peso_Ponderado_N5, mi.Fecha_Inicial, mi.Fecha_Final, mi.Es_Finalizable, mi.Id_Detalle_Estado, md.Descripcion AS DescripcionMetaDepartamental, ae.Id_Area_Ejecutora, ae.Nombre AS NombreAreaEjecutora, m.Num_Meta AS NumMetaPadre, o.Num_Objetivo AS NumObjetivoPadre, o.Nombre AS NombreObjetivoPadre, t.Id_Tipo_Objetivo, t.Nombre AS NombreTipoObjetivo, de.Descripcion AS DescripcionEstado FROM dbo.Meta_Individual mi INNER JOIN dbo.Meta_Departamental md ON mi.Id_Meta_Departamental = md.Id_Meta_Departamental INNER JOIN dbo.Meta m ON md.Id_Meta = m.Id_Meta INNER JOIN dbo.Objetivo o ON m.Id_Objetivo = o.Id_Objetivo LEFT JOIN dbo.Tipo_Objetivo t ON o.Id_Tipo_Objetivo = t.Id_Tipo_Objetivo LEFT JOIN dbo.Area_Ejecutora ae ON md.Id_Area_Ejecutora = ae.Id_Area_Ejecutora LEFT JOIN dbo.Detalle_Estado de ON mi.Id_Detalle_Estado = de.Id_Detalle_Estado WHERE mi.Id_Meta_Individual = @IdMetaIndividual";
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMetaIndividual", idMetaIndividual);
                    try { con.Open(); using (SqlDataReader reader = cmd.ExecuteReader()) { if (reader.Read()) { metaInd = new MetaIndividualInfo { /* ... mapping ... */ IdMetaIndividual = Convert.ToInt32(reader["Id_Meta_Individual"]), IdMetaDepartamental = reader["Id_Meta_Departamental"] != DBNull.Value ? Convert.ToInt32(reader["Id_Meta_Departamental"]) : (int?)null, Usuario = reader["Usuario"]?.ToString() ?? string.Empty, Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty, Alcance = reader["Alcance"]?.ToString() ?? string.Empty, PesoPonderadoN4 = reader["Peso_Ponderado_N4"] != DBNull.Value ? Convert.ToInt32(reader["Peso_Ponderado_N4"]) : (int?)null, PesoPonderadoN5 = reader["Peso_Ponderado_N5"] != DBNull.Value ? Convert.ToInt32(reader["Peso_Ponderado_N5"]) : (int?)null, FechaInicial = reader["Fecha_Inicial"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Inicial"]) : (DateTime?)null, FechaFinal = reader["Fecha_Final"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Final"]) : (DateTime?)null, EsFinalizable = reader["Es_Finalizable"] != DBNull.Value ? Convert.ToBoolean(reader["Es_Finalizable"]) : (bool?)null, IdDetalleEstado = reader["Id_Detalle_Estado"] != DBNull.Value ? Convert.ToInt32(reader["Id_Detalle_Estado"]) : (int?)null, DescripcionMetaDepartamental = reader["DescripcionMetaDepartamental"]?.ToString() ?? "N/A", IdAreaEjecutora = reader["Id_Area_Ejecutora"] != DBNull.Value ? Convert.ToInt32(reader["Id_Area_Ejecutora"]) : (int?)null, NombreAreaEjecutora = reader["NombreAreaEjecutora"]?.ToString() ?? "N/A", NumMetaPadre = reader["NumMetaPadre"] != DBNull.Value ? Convert.ToInt32(reader["NumMetaPadre"]) : (int?)null, NumObjetivoPadre = reader["NumObjetivoPadre"] != DBNull.Value ? Convert.ToInt32(reader["NumObjetivoPadre"]) : (int?)null, NombreObjetivoPadre = reader["NombreObjetivoPadre"]?.ToString() ?? "N/A", IdTipoObjetivo = reader["Id_Tipo_Objetivo"] != DBNull.Value ? Convert.ToInt32(reader["Id_Tipo_Objetivo"]) : (int?)null, NombreTipoObjetivo = reader["NombreTipoObjetivo"]?.ToString() ?? "N/A", DescripcionEstado = reader["DescripcionEstado"]?.ToString() ?? "N/A" }; } } }
                    catch (Exception ex) { Console.WriteLine("Error en ObtenerMetaIndividualPorId: " + ex.Message); }
                }
            }
            return metaInd;
        }
        // Method to get distinct users from Meta_Individual
        public List<string> ObtenerUsuariosUnicos()
        {
            List<string> usuarios = new List<string>();
            string query = "SELECT DISTINCT Usuario FROM dbo.Meta_Individual WHERE Usuario IS NOT NULL ORDER BY Usuario";

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
                                usuarios.Add(reader["Usuario"].ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerUsuariosUnicos: " + ex.Message);
                        throw;
                    }
                }
            }
            return usuarios;
        }

    } // End of MetaIndividualDAL class

}