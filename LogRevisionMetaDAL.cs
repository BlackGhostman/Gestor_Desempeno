using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Gestor_Desempeno
{
    // Nueva clase LogRevisionMetaDAL.cs
    public class LogRevisionMetaDAL
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }

        public bool GuardarLogRevision(int idMetaIndividual, string idUsuarioJefe, string comentarioJefe,
                                       int? estadoMetaAntes, int? estadoMetaDespues, string tipoAccion)
        {
            string query = @"INSERT INTO dbo.Log_Revision_Meta 
                           (IdMetaIndividual, IdUsuarioJefe, ComentarioJefe, EstadoMetaAntesRevision, EstadoMetaDespuesRevision, TipoAccion, FechaRevision)
                         VALUES 
                           (@IdMetaIndividual, @IdUsuarioJefe, @ComentarioJefe, @EstadoMetaAntes, @EstadoMetaDespues, @TipoAccion, GETDATE())";
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdMetaIndividual", idMetaIndividual);
                    cmd.Parameters.AddWithValue("@IdUsuarioJefe", idUsuarioJefe);
                    cmd.Parameters.AddWithValue("@ComentarioJefe", string.IsNullOrWhiteSpace(comentarioJefe) ? (object)DBNull.Value : comentarioJefe);
                    cmd.Parameters.AddWithValue("@EstadoMetaAntes", (object)estadoMetaAntes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EstadoMetaDespues", (object)estadoMetaDespues ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TipoAccion", tipoAccion);
                    try
                    {
                        con.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                    catch (Exception ex) { /* Log error */ return false; }
                }
            }
        }
    }
}