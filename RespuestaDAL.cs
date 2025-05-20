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
    }

   

    public class RespuestaDAL
    {
        private string GetConnectionString()
        {
            // Ensure this connection string name matches your Web.config
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }

        // Generates week code in WMMYYYY format (e.g., 2052025 for Week 2, May 2025)
        private string GetCodigoSemana_WMMYYYY(int anio,int mes,int Semana)
        {
            string mesi = "";
            if(mes < 10)
            {
                mesi = "0" + mes.ToString();
            }
            else
            {
                mesi = mes.ToString();
            }
            int weekOfMonth = Semana;
            string monthStr = mesi; // Format month with leading zero
            return $"{weekOfMonth}{monthStr}{anio}";
        }

        // Helper to get week of month (approximate, starts week on Monday)
        private int GetWeekOfMonth(DateTime date)
        {
            DateTime firstOfMonth = new DateTime(date.Year, date.Month, 1);
            int firstDayOfWeek = ((int)firstOfMonth.DayOfWeek + 6) % 7; // Monday = 0
            int weekNum = (date.Day + firstDayOfWeek - 1) / 7 + 1;
            return Math.Min(weekNum, 5); // Cap at week 5
        }

        // Method to get a specific weekly response
        public RespuestaInfo ObtenerRespuestaSemanal(int idMetaIndividual, string codigoSemana)
        {
            RespuestaInfo respuesta = null;
            if (string.IsNullOrEmpty(codigoSemana)) return null;

            // NombreArchivo is NOT selected as it's not in the DB
            string query = @"SELECT Id_Respuesta, Id_Meta_Individual, Descripcion, Fecha_Entregado, Id_Detalle_Estado, Codigo_Semana
                             FROM dbo.Respuesta
                             WHERE Id_Meta_Individual = @IdMetaIndividual AND Codigo_Semana = @CodigoSemana";

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
                                    // NombreArchivo is not mapped from DB
                                };
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("Error en ObtenerRespuestaSemanal: " + ex.Message); }
                }
            }
            return respuesta;
        }


        // Inserts or Updates a response based on whether one exists for the metaId and, for weekly, the CodigoSemana.
        // Returns the ID of the saved response or -1 on failure.
        public int GuardarRespuesta(int idMetaIndividual, string descripcion, int idDetalleEstadoDestino, bool esMetaFinalizable, string nombreArchivo = null, string codigoSemanaEspecifico = null)
        {
            int respuestaId = -1;
            DateTime fechaEvento = DateTime.Now;
            string codigoSemanaParaGuardar = null; // This will be NULL for finalizable, or WMMYYYY for weekly
            RespuestaInfo respuestaExistente = null;

            // Determine logic based on meta type
            if (esMetaFinalizable)
            {
                // Finalizable: Check if *any* response exists for this meta
                // CodigoSemana will be NULL for these.
                respuestaExistente = ObtenerRespuestaPorMetaId(idMetaIndividual); // This method should ideally fetch where CodigoSemana IS NULL
                codigoSemanaParaGuardar = null;
            }
            else
            {
                // Weekly: Calculate week code and check for response *for that specific week*
                
                codigoSemanaParaGuardar = GetCodigoSemana_WMMYYYY(fechaEvento.Year,fechaEvento.Month, Convert.ToInt32(codigoSemanaEspecifico));
                respuestaExistente = ObtenerRespuestaSemanal(idMetaIndividual, codigoSemanaParaGuardar);
            }

            string query;
            if (respuestaExistente != null)
            {
                // --- UPDATE ---
                respuestaId = respuestaExistente.IdRespuesta;
                query = @"UPDATE dbo.Respuesta SET
                             Descripcion = @Descripcion,
                             Fecha_Entregado = @FechaEntregado, -- Always update timestamp
                             Id_Detalle_Estado = @IdDetalleEstado
                             -- Codigo_Semana should not change on update for an existing weekly record
                             -- NombreArchivo is not a DB column
                          WHERE Id_Respuesta = @IdRespuesta";
            }
            else
            {
                // --- INSERT ---
                query = @"INSERT INTO dbo.Respuesta
                             (Id_Meta_Individual, Descripcion, Fecha_Entregado, Id_Detalle_Estado, Codigo_Semana) -- Removed NombreArchivo
                          VALUES
                             (@IdMetaIndividual, @Descripcion, @FechaEntregado, @IdDetalleEstado, @CodigoSemana);
                          SELECT SCOPE_IDENTITY();";
            }

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    // Parameters common to INSERT and UPDATE (except IdRespuesta)
                    cmd.Parameters.AddWithValue("@IdMetaIndividual", idMetaIndividual);
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : (object)descripcion);
                    cmd.Parameters.AddWithValue("@FechaEntregado", fechaEvento);
                    cmd.Parameters.AddWithValue("@IdDetalleEstado", idDetalleEstadoDestino);

                    if (respuestaExistente != null) // UPDATE
                    {
                        cmd.Parameters.AddWithValue("@IdRespuesta", respuestaId);
                        // For UPDATE, Codigo_Semana is part of the WHERE clause in ObtenerRespuestaSemanal,
                        // so we don't set it again here. The UPDATE query doesn't include it in SET.
                        // The UPDATE query also doesn't include NombreArchivo.
                    }
                    else // INSERT
                    {
                        cmd.Parameters.AddWithValue("@CodigoSemana", (object)codigoSemanaParaGuardar ?? DBNull.Value);
                        // NombreArchivo is not saved to DB
                    }


                    try
                    {
                        con.Open();
                        if (respuestaExistente != null) // Execute Update
                        {
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected == 0) respuestaId = -1;
                        }
                        else // Execute Insert
                        {
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value) { respuestaId = Convert.ToInt32(result); }
                            else { respuestaId = -1; } // Insert failed
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in GuardarRespuesta (MetaID: {idMetaIndividual}): {ex.Message}");
                        throw;
                    }
                }
            }
            return respuestaId; // Return the ID of the saved record or -1 on failure
        }


        // Gets the response for a specific Meta Individual ID.
        // For finalizable metas, it's the single response.
        // For weekly metas, it should ideally get the response for the *current* or *specified* week if context is available,
        // or the most recent one as a fallback.
        // The WebMethod GetRespuestaDetalles will now pass CodigoSemanaDeLaPestana for weekly metas.
        public RespuestaInfo ObtenerRespuestaPorMetaId(int idMetaIndividual)
        {
            RespuestaInfo respuesta = null;
            // This query gets the LATEST response if multiple exist (e.g. if CodigoSemana was not always used or for finalizable)
            // For finalizable metas (CodigoSemana IS NULL), this is correct.
            // Removed NombreArchivo from SELECT
            string query = @"SELECT TOP 1 Id_Respuesta, Id_Meta_Individual, Descripcion, Fecha_Entregado, Id_Detalle_Estado, Codigo_Semana
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
                                    // NombreArchivo not mapped
                                };
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("Error en ObtenerRespuestaPorMetaId: " + ex.Message); }
                }
            }
            return respuesta;
        }

        // ** NEW METHOD **
        // Gets the IDs of MetaIndividual records that have a response with a specific state ID.
        public HashSet<int> ObtenerIdsMetasConEstadoRespuesta(List<int> idsMetasIndividuales, int idDetalleEstadoRespuesta)
        {
            HashSet<int> idsConRespuesta = new HashSet<int>();
            if (idsMetasIndividuales == null || !idsMetasIndividuales.Any())
            {
                return idsConRespuesta; // Return empty set if no IDs provided
            }

            // Create comma-separated string of IDs for the IN clause
            string idList = string.Join(",", idsMetasIndividuales);

            // Use string interpolation carefully or preferably use a Table-Valued Parameter for large lists
            // Using IN clause for simplicity here, but be mindful of SQL Server limits on IN clause size.
            string query = $@"SELECT DISTINCT Id_Meta_Individual
                              FROM dbo.Respuesta
                              WHERE Id_Meta_Individual IN ({idList})
                              AND Id_Detalle_Estado = @IdDetalleEstado";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    // Add the state ID parameter
                    cmd.Parameters.AddWithValue("@IdDetalleEstado", idDetalleEstadoRespuesta);

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
                        // Consider re-throwing or logging more details
                    }
                }
            }
            return idsConRespuesta;
        }


        // Deletes a specific response record by its ID
        public bool EliminarRespuesta(int idRespuesta)
        {
            if (idRespuesta <= 0) return false;
            bool eliminado = false;
            string query = "DELETE FROM dbo.Respuesta WHERE Id_Respuesta = @IdRespuesta";
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdRespuesta", idRespuesta);
                    try { con.Open(); int rowsAffected = cmd.ExecuteNonQuery(); eliminado = (rowsAffected > 0); }
                    catch (Exception ex) { Console.WriteLine("Error en EliminarRespuesta: " + ex.Message); throw; }
                }
            }
            return eliminado;
        }

        // Static version of GetCodigoSemana_WMMYYYY for access from WebMethod if DAL instance isn't available
        // This is a helper for the WebMethod, not directly part of DAL's primary responsibility
        public string GetCodigoSemana_WMMYYYY_Static(DateTime fecha,int numSemana)
        {

            return GetCodigoSemana_WMMYYYY(fecha.Year,fecha.Month, numSemana);
        }

    } // End of RespuestaDAL class
}