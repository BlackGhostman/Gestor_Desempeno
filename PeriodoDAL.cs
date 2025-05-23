using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Gestor_Desempeno
{
    public class PeriodoInfo
    {
        public int Id_Periodo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Estado { get; set; } // true = Activo, false = Inactivo
        public DateTime? Fecha_Inicio { get; set; } // Nueva propiedad, DateTime? para permitir nulos
        public DateTime? Fecha_Final { get; set; }   // Nueva propiedad, DateTime? para permitir nulos
    }

    public class PeriodoDAL
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }

        public List<PeriodoInfo> ObtenerPeriodos(bool soloActivos = true)
        {
            List<PeriodoInfo> listaPeriodos = new List<PeriodoInfo>();
            // Se agregan las nuevas columnas a la consulta SELECT
            string query = "SELECT Id_Periodo, Nombre, Descripcion, Estado, Fecha_Inicio, Fecha_Final FROM dbo.Periodo";
            if (soloActivos)
            {
                query += " WHERE Estado = 1";
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
                                PeriodoInfo periodo = new PeriodoInfo
                                {
                                    Id_Periodo = Convert.ToInt32(reader["Id_Periodo"]),
                                    Nombre = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString() : string.Empty,
                                    Descripcion = reader["Descripcion"] != DBNull.Value ? reader["Descripcion"].ToString() : string.Empty,
                                    Estado = Convert.ToBoolean(reader["Estado"]),
                                    // Leer las nuevas columnas de fecha, manejando DBNull.Value
                                    Fecha_Inicio = reader["Fecha_Inicio"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Inicio"]) : (DateTime?)null,
                                    Fecha_Final = reader["Fecha_Final"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Final"]) : (DateTime?)null
                                };
                                listaPeriodos.Add(periodo);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerPeriodos: " + ex.Message);
                        // Considera un logging más robusto aquí
                    }
                }
            }
            return listaPeriodos;
        }

        public bool InsertarPeriodo(string nombre, string descripcion, bool estado, DateTime? fechaInicio, DateTime? fechaFinal)
        {
            bool insertado = false;
            // Se agregan las nuevas columnas y parámetros a la consulta INSERT
            string query = @"INSERT INTO dbo.Periodo (Nombre, Descripcion, Estado, Fecha_Inicio, Fecha_Final) 
                             VALUES (@Nombre, @Descripcion, @Estado, @Fecha_Inicio, @Fecha_Final)";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", estado);
                    // Agregar parámetros para las nuevas fechas, manejando nulos
                    cmd.Parameters.AddWithValue("@Fecha_Inicio", (object)fechaInicio ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fecha_Final", (object)fechaFinal ?? DBNull.Value);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            insertado = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en InsertarPeriodo: " + ex.Message);
                    }
                }
            }
            return insertado;
        }

        public bool ActualizarPeriodo(int Id_Periodo, string nombre, string descripcion, bool estado, DateTime? fechaInicio, DateTime? fechaFinal)
        {
            bool actualizado = false;
            // Se agregan las nuevas columnas y parámetros a la consulta UPDATE
            string query = @"UPDATE dbo.Periodo 
                             SET Nombre = @Nombre, Descripcion = @Descripcion, Estado = @Estado, 
                                 Fecha_Inicio = @Fecha_Inicio, Fecha_Final = @Fecha_Final
                             WHERE Id_Periodo = @Id_Periodo";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", estado);
                    cmd.Parameters.AddWithValue("@Id_Periodo", Id_Periodo);
                    // Agregar parámetros para las nuevas fechas, manejando nulos
                    cmd.Parameters.AddWithValue("@Fecha_Inicio", (object)fechaInicio ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fecha_Final", (object)fechaFinal ?? DBNull.Value);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            actualizado = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ActualizarPeriodo: " + ex.Message);
                    }
                }
            }
            return actualizado;
        }

        public bool EliminarPeriodoLogico(int Id_Periodo)
        {
            bool eliminado = false;
            string query = "UPDATE dbo.Periodo SET Estado = 0 WHERE Id_Periodo = @Id_Periodo";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id_Periodo", Id_Periodo);
                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            eliminado = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en EliminarPeriodoLogico: " + ex.Message);
                    }
                }
            }
            return eliminado;
        }

        public PeriodoInfo ObtenerPeriodoPorId(int Id_Periodo)
        {
            PeriodoInfo periodo = null;
            // Se agregan las nuevas columnas a la consulta SELECT
            string query = "SELECT Id_Periodo, Nombre, Descripcion, Estado, Fecha_Inicio, Fecha_Final FROM dbo.Periodo WHERE Id_Periodo = @Id_Periodo";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id_Periodo", Id_Periodo);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                periodo = new PeriodoInfo
                                {
                                    Id_Periodo = Convert.ToInt32(reader["Id_Periodo"]),
                                    Nombre = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString() : string.Empty,
                                    Descripcion = reader["Descripcion"] != DBNull.Value ? reader["Descripcion"].ToString() : string.Empty,
                                    Estado = Convert.ToBoolean(reader["Estado"]),
                                    Fecha_Inicio = reader["Fecha_Inicio"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Inicio"]) : (DateTime?)null,
                                    Fecha_Final = reader["Fecha_Final"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Final"]) : (DateTime?)null
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerPeriodoPorId: " + ex.Message);
                    }
                }
            }
            return periodo;
        }
    }
}