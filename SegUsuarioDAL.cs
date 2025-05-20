// DAL/SegUsuarioDAL.cs
// Clase de Acceso a Datos para la tabla SEG_USUARIO.
// Actualizado para incluir filtro por nombre en ObtenerTodosLosUsuarios.
using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Text; // Para StringBuilder

namespace Gestor_Desempeno
{
    public class SegUsuarioInfo
    {
        public string ID_USUARIO { get; set; }
        public string NOMBRE { get; set; }
        public string PRIMER_APELLIDO { get; set; }
        public string SEGUNDO_APELLIDO { get; set; }
        public string NOMBRE_COMPLETO
        {
            get
            {
                // Construir el nombre completo, manejando posibles nulos o vacíos
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(NOMBRE)) parts.Add(NOMBRE);
                if (!string.IsNullOrWhiteSpace(PRIMER_APELLIDO)) parts.Add(PRIMER_APELLIDO);
                if (!string.IsNullOrWhiteSpace(SEGUNDO_APELLIDO)) parts.Add(SEGUNDO_APELLIDO);
                return string.Join(" ", parts);
            }
        }
        public string LinkFoto { get; set; }
    }

    public class SegUsuarioDAL
    {
        // Obtiene todos los usuarios, opcionalmente filtrados por nombre completo.
        public static DataTable ObtenerTodosLosUsuarios(string connectionString, string filtroNombre = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                StringBuilder queryBuilder = new StringBuilder();
                queryBuilder.Append(@"
                    SELECT 
                        ID_USUARIO, 
                        NOMBRE, 
                        PRIMER_APELLIDO, 
                        SEGUNDO_APELLIDO, 
                        LTRIM(RTRIM(ISNULL(NOMBRE, '') + ' ' + ISNULL(PRIMER_APELLIDO, '') + ' ' + ISNULL(SEGUNDO_APELLIDO, ''))) AS NOMBRE_COMPLETO,
                        LinkFoto 
                    FROM 
                        dbo.SEG_USUARIO");

                if (!string.IsNullOrWhiteSpace(filtroNombre))
                {
                    // Usar CONCAT para el filtro para manejar mejor los nulos en los componentes del nombre.
                    // Y añadir comodines para búsqueda parcial.
                    queryBuilder.Append(" WHERE CONCAT(ISNULL(NOMBRE, ''), ' ', ISNULL(PRIMER_APELLIDO, ''), ' ', ISNULL(SEGUNDO_APELLIDO, '')) LIKE @FiltroNombre");
                }

                queryBuilder.Append(" ORDER BY NOMBRE, PRIMER_APELLIDO, SEGUNDO_APELLIDO");

                using (SqlCommand cmd = new SqlCommand(queryBuilder.ToString(), con))
                {
                    cmd.CommandType = CommandType.Text;
                    if (!string.IsNullOrWhiteSpace(filtroNombre))
                    {
                        cmd.Parameters.AddWithValue("@FiltroNombre", "%" + filtroNombre + "%");
                    }

                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception("Error de base de datos al obtener usuarios: " + ex.Message, ex);
                    }
                    finally
                    {
                        if (con.State == ConnectionState.Open)
                        {
                            con.Close();
                        }
                    }
                }
            }
            return dt;
        }

        public static DataRow ObtenerUsuarioPorId(string idUsuario, string connectionString)
        {
            DataTable dt = new DataTable();
            DataRow dr = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        ID_USUARIO, 
                        NOMBRE, 
                        PRIMER_APELLIDO, 
                        SEGUNDO_APELLIDO, 
                        LinkFoto 
                    FROM 
                        dbo.SEG_USUARIO 
                    WHERE 
                        ID_USUARIO = @ID_USUARIO";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ID_USUARIO", idUsuario);
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            dr = dt.Rows[0];
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception("Error de base de datos al obtener usuario por ID: " + ex.Message, ex);
                    }
                    finally
                    {
                        if (con.State == ConnectionState.Open)
                        {
                            con.Close();
                        }
                    }
                }
            }
            return dr;
        }

        public static void ActualizarLinkFoto(string idUsuario, string linkFoto, string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                linkFoto = linkFoto.Replace("Fotos/", "");
                string query = @"
                    UPDATE dbo.SEG_USUARIO 
                    SET LinkFoto = @LinkFoto 
                    WHERE ID_USUARIO = @ID_USUARIO";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@LinkFoto", (object)linkFoto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ID_USUARIO", idUsuario);
                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception("Error de base de datos al actualizar LinkFoto: " + ex.Message, ex);
                    }
                    finally
                    {
                        if (con.State == ConnectionState.Open)
                        {
                            con.Close();
                        }
                    }
                }
            }
        }
    }
}
