using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization; // Para Calendar/WeekRule

namespace Gestor_Desempeno
{
    public class RespuestaInfo
    {
        public int IdRespuesta { get; set; }
        public int? IdMetaIndividual { get; set; }
        public string Descripcion { get; set; } // Observación
        public DateTime? FechaEntregado { get; set; }
        public int? IdDetalleEstado { get; set; }
        public string CodigoSemana { get; set; } // nvarchar(10)
        public string NombreArchivo { get; set; } // For future use, not a DB column

        public string DatosDocs { get; set; }
    }


    public class RespuestaDAL
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }


        // En tu clase RespuestaDAL dentro de RespuestaDAL.cs

        // Modifica la firma del método para incluir codigoSemanaFiltro
        public List<RespuestaInfo> ObtenerHistorialRespuestas(int idMetaIndividual, string codigoSemanaFiltro = null)
        {
            List<RespuestaInfo> historial = new List<RespuestaInfo>();

            string query = @"SELECT Id_Respuesta, Id_Meta_Individual, Descripcion, Fecha_Entregado, Id_Detalle_Estado, Codigo_Semana 
                     FROM dbo.Respuesta 
                     WHERE Id_Meta_Individual = @IdMetaIndividual";

            // Si se proporciona un codigoSemanaFiltro y NO es "0000000" (que significa mostrar todo para vencidas),
            // entonces filtramos por ese código de semana.
            if (!string.IsNullOrEmpty(codigoSemanaFiltro) && codigoSemanaFiltro != "0000000")
            {
                query += " AND Codigo_Semana = @CodigoSemanaFiltro";
            }

            query += " ORDER BY Fecha_Entregado DESC";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMetaIndividual", idMetaIndividual);

                    if (!string.IsNullOrEmpty(codigoSemanaFiltro) && codigoSemanaFiltro != "0000000")
                    {
                        cmd.Parameters.AddWithValue("@CodigoSemanaFiltro", codigoSemanaFiltro);
                    }

                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                historial.Add(new RespuestaInfo
                                {
                                    IdRespuesta = Convert.ToInt32(reader["Id_Respuesta"]),
                                    IdMetaIndividual = reader["Id_Meta_Individual"] != DBNull.Value ? Convert.ToInt32(reader["Id_Meta_Individual"]) : (int?)null,
                                    Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                                    FechaEntregado = reader["Fecha_Entregado"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Entregado"]) : (DateTime?)null,
                                    IdDetalleEstado = reader["Id_Detalle_Estado"] != DBNull.Value ? Convert.ToInt32(reader["Id_Detalle_Estado"]) : (int?)null,
                                    CodigoSemana = reader["Codigo_Semana"]?.ToString()
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en RespuestaDAL.ObtenerHistorialRespuestas: {ex.Message}");
                        throw;
                    }
                }
            }
            return historial;
        }

        // Nuevo método para determinar la fecha de la última respuesta del subordinado (para "Respondida Fuera de Tiempo")
        public DateTime? ObtenerFechaUltimaRespuestaSubordinado(int idMetaIndividual)
        {
            DateTime? fecha = null;
            // Esta consulta asume que la "última respuesta" del subordinado que importa es la que tiene
            // un estado de respuesta "Respondido" (ID 11 para la clase Respuesta) o es la más reciente.
            // Podría necesitar ajustes según cómo exactamente se marca una meta como "lista para revisión del jefe".
            string query = @"SELECT TOP 1 Fecha_Entregado 
                     FROM dbo.Respuesta
                     WHERE Id_Meta_Individual = @IdMetaIndividual
                             ORDER BY Fecha_Entregado DESC";
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMetaIndividual", idMetaIndividual);
                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            fecha = Convert.ToDateTime(result);
                        }
                    }
                    catch (Exception ex) { /* Log */ }
                }
            }
            return fecha;
        }

        // Constantes para los estados (es mejor tenerlas definidas en un solo lugar, pero las replico aquí para claridad del ejemplo)
        private const int ID_ESTADO_ACTIVO_SEMANAL_DAL = 9; // Estado para avance o respuesta semanal activa
        private const int ID_ESTADO_RESPONDIDO_DAL = 11;    // Estado para meta finalizada

        // Tu método GetCodigoSemana_WMMYYYY (parece correcto, pero no se usará dentro de GuardarRespuesta si el parámetro ya viene formateado)
        private string GetCodigoSemana_WMMYYYY(int anio, int mes, int semanaNum)
        {
            string mesStr = mes < 10 ? "0" + mes.ToString() : mes.ToString();
            return $"{semanaNum}{mesStr}{anio}";
        }

        public RespuestaInfo ObtenerRespuestaSemanal(int idMetaIndividual, string codigoSemana)
        {
            RespuestaInfo respuesta = null;
            if (string.IsNullOrEmpty(codigoSemana)) return null;

            string query = @"SELECT Id_Respuesta, Id_Meta_Individual, Descripcion, Fecha_Entregado, Id_Detalle_Estado, Codigo_Semana
                         FROM dbo.Respuesta
                         WHERE Id_Meta_Individual = @IdMetaIndividual ";//AND Codigo_Semana = @CodigoSemana

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMetaIndividual", idMetaIndividual);
                    cmd.Parameters.AddWithValue("@CodigoSemana", codigoSemana);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                respuesta = new RespuestaInfo
                                {
                                    IdRespuesta = Convert.ToInt32(reader["Id_Respuesta"]),
                                    IdMetaIndividual = reader["Id_Meta_Individual"] != DBNull.Value ? Convert.ToInt32(reader["Id_Meta_Individual"]) : (int?)null,
                                    Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                                    FechaEntregado = reader["Fecha_Entregado"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Entregado"]) : (DateTime?)null,
                                    IdDetalleEstado = reader["Id_Detalle_Estado"] != DBNull.Value ? Convert.ToInt32(reader["Id_Detalle_Estado"]) : (int?)null,
                                    CodigoSemana = reader["Codigo_Semana"]?.ToString()
                                };
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("Error en ObtenerRespuestaSemanal: " + ex.Message); throw; }
                }
            }
            return respuesta;
        }

        public RespuestaInfo ObtenerRespuestaPorMetaId(int idMetaIndividual)
        {
            RespuestaInfo respuesta = null;
            // Obtiene la respuesta donde Codigo_Semana es NULL (típicamente para metas finalizadas que no son por semana)
            // O la más reciente si hay varias (aunque para finalizadas debería haber una).
            string query = @"SELECT TOP 1 Id_Respuesta, Id_Meta_Individual, Descripcion, Fecha_Entregado, Id_Detalle_Estado, Codigo_Semana
                         FROM dbo.Respuesta
                         WHERE Id_Meta_Individual = @IdMetaIndividual 
                         AND Codigo_Semana IS NULL  -- Importante para distinguir de avances semanales de metas finalizables
                         ORDER BY Fecha_Entregado DESC";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMetaIndividual", idMetaIndividual);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                respuesta = new RespuestaInfo
                                {
                                    IdRespuesta = Convert.ToInt32(reader["Id_Respuesta"]),
                                    IdMetaIndividual = reader["Id_Meta_Individual"] != DBNull.Value ? Convert.ToInt32(reader["Id_Meta_Individual"]) : (int?)null,
                                    Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                                    FechaEntregado = reader["Fecha_Entregado"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Entregado"]) : (DateTime?)null,
                                    IdDetalleEstado = reader["Id_Detalle_Estado"] != DBNull.Value ? Convert.ToInt32(reader["Id_Detalle_Estado"]) : (int?)null,
                                    CodigoSemana = reader["Codigo_Semana"]?.ToString() // Debería ser NULL aquí
                                };
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("Error en ObtenerRespuestaPorMetaId (buscando respuesta final): " + ex.Message); throw; }
                }
            }
            return respuesta;
        }

        // El parámetro 'codigoSemanaEspecifico' viene de Desempeno.aspx.cs y puede ser:
        // 1. Formato WMMYYYY: Para avances de metas finalizables (estado ACTIVO_SEMANAL).
        // 2. Formato WMMYYYY: Para respuestas de metas semanales no finalizables.
        // 3. NULL: Cuando se está finalizando completamente una meta finalizable (estado RESPONDIDO).
        public int GuardarRespuesta(int idMetaIndividual, string descripcion, int idDetalleEstadoDestino, bool esMetaFinalizable, string nombreArchivo = null, string codigoSemanaEspecifico = null)
        {
            int respuestaId = -1;
            DateTime fechaEvento = DateTime.Now;
            RespuestaInfo respuestaExistente = null;

            // Determinar si existe una respuesta previa para actualizarla
            if (esMetaFinalizable)
            {
                if (idDetalleEstadoDestino == ID_ESTADO_RESPONDIDO_DAL) // Finalizando la meta
                {
                    // Busca una respuesta "final" (sin código de semana) para esta meta.
                    respuestaExistente = ObtenerRespuestaPorMetaId(idMetaIndividual);
                }
                else if (idDetalleEstadoDestino == ID_ESTADO_ACTIVO_SEMANAL_DAL && !string.IsNullOrWhiteSpace(codigoSemanaEspecifico)) // Guardando avance para una meta finalizable
                {
                    // Busca un avance para esta meta Y esta semana específica.
                    respuestaExistente = ObtenerRespuestaSemanal(idMetaIndividual, codigoSemanaEspecifico);
                }
                // Si es finalizable, estado de avance, pero no hay codigoSemanaEspecifico, es un caso anómalo, se tratará como INSERT sin CodigoSemana (o podría fallar).
                // Desempeno.aspx.cs debería proveer el codigoSemanaEspecifico para avances.
            }
            else // Meta no finalizable (semanal pura)
            {
                if (!string.IsNullOrWhiteSpace(codigoSemanaEspecifico))
                {
                    respuestaExistente = ObtenerRespuestaSemanal(idMetaIndividual, codigoSemanaEspecifico);
                }
                else
                {
                    // Error: las metas semanales no finalizables siempre deben tener un codigoSemana.
                    Console.WriteLine($"Error: Se intentó guardar una respuesta para una meta semanal (ID: {idMetaIndividual}) sin CodigoSemana.");
                    return -1; // O lanzar excepción
                }
            }

            string query;
            if (respuestaExistente != null)
            {
                // --- UPDATE ---
                respuestaId = respuestaExistente.IdRespuesta;
                query = @"UPDATE dbo.Respuesta SET
                            Descripcion = @Descripcion,
                            Fecha_Entregado = @FechaEntregado,
                            Id_Detalle_Estado = @IdDetalleEstado
                      WHERE Id_Respuesta = @IdRespuesta";
            }
            else
            {
                // --- INSERT ---
                // Nombre_Archivo no se incluye en el INSERT ya que no es una columna de DB según tu RespuestaInfo.
                query = @"INSERT INTO dbo.Respuesta 
                            (Id_Meta_Individual, Descripcion, Fecha_Entregado, Id_Detalle_Estado, Codigo_Semana)
                      VALUES 
                            (@IdMetaIndividual, @Descripcion, @FechaEntregado, @IdDetalleEstado, @CodigoSemanaParam);
                      SELECT SCOPE_IDENTITY();";
            }

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMetaIndividual", idMetaIndividual);
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : (object)descripcion);
                    cmd.Parameters.AddWithValue("@FechaEntregado", fechaEvento);
                    cmd.Parameters.AddWithValue("@IdDetalleEstado", idDetalleEstadoDestino);

                    if (respuestaExistente != null) // Parámetros para UPDATE
                    {
                        cmd.Parameters.AddWithValue("@IdRespuesta", respuestaId);
                    }
                    else // Parámetros para INSERT
                    {
                        // El valor de 'codigoSemanaEspecifico' es el que se debe guardar.
                        // Si es para "Finalizar Meta", Desempeno.aspx.cs envía null.
                        // Si es para "Guardar Avance" o "Guardar Semanal", Desempeno.aspx.cs envía WMMYYYY.
                        cmd.Parameters.AddWithValue("@CodigoSemanaParam", (object)codigoSemanaEspecifico ?? DBNull.Value);
                    }

                    try
                    {
                        con.Open();
                        if (respuestaExistente != null) // Ejecutar UPDATE
                        {
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                respuestaId = -1; // No se actualizó nada, podría ser un error o condición no encontrada.
                                Console.WriteLine($"Warning: Update no afectó filas para IdRespuesta {respuestaExistente.IdRespuesta}");
                            }
                            // respuestaId ya tiene el ID correcto de respuestaExistente.IdRespuesta
                        }
                        else // Ejecutar INSERT
                        {
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                respuestaId = Convert.ToInt32(result);
                            }
                            else
                            {
                                respuestaId = -1; // Falló el INSERT o SCOPE_IDENTITY() no devolvió nada.
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en RespuestaDAL.GuardarRespuesta (MetaID: {idMetaIndividual}, CodigoSemana: {codigoSemanaEspecifico}): {ex.ToString()}");
                        throw; // Relanzar para que la capa superior sepa del error.
                    }
                }
            }
            return respuestaId;
        }

        public HashSet<int> ObtenerIdsMetasConEstadoRespuesta(List<int> idsMetasIndividuales, int idDetalleEstadoRespuesta)
        {
            HashSet<int> idsConRespuesta = new HashSet<int>();
            if (idsMetasIndividuales == null || !idsMetasIndividuales.Any())
            {
                return idsConRespuesta;
            }

            var parameters = new List<string>();
            var sqlParameters = new List<SqlParameter>();

            for (int i = 0; i < idsMetasIndividuales.Count; i++)
            {
                var paramName = $"@MetaId{i}";
                parameters.Add(paramName);
                sqlParameters.Add(new SqlParameter(paramName, idsMetasIndividuales[i]));
            }

            string query = $@"SELECT DISTINCT Id_Meta_Individual
                          FROM dbo.Respuesta
                          WHERE Id_Meta_Individual IN ({string.Join(",", parameters)})
                          AND Id_Detalle_Estado = @IdDetalleEstado";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdDetalleEstado", idDetalleEstadoRespuesta);
                    cmd.Parameters.AddRange(sqlParameters.ToArray());

                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader["Id_Meta_Individual"] != DBNull.Value)
                                {
                                    idsConRespuesta.Add(Convert.ToInt32(reader["Id_Meta_Individual"]));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerIdsMetasConEstadoRespuesta: " + ex.Message);
                        throw;
                    }
                }
            }
            return idsConRespuesta;
        }
    }

}