using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Gestor_Desempeno
{


    // Define una clase simple para representar un Periodo
    // Puedes crear un archivo separado para esta clase si prefieres
    public class PeriodoInfo
    {
        public int Id_Periodo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Estado { get; set; } // true = Activo, false = Inactivo
    }


    public class PeriodoDAL
    {
        // Obtener la cadena de conexión desde Web.config
        private string GetConnectionString()
        {
            // Asegúrate de usar el nombre correcto de la connection string
            return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
        }

        // Método para obtener todos los periodos activos o todos
        public List<PeriodoInfo> ObtenerPeriodos(bool soloActivos = true)
        {
            List<PeriodoInfo> listaPeriodos = new List<PeriodoInfo>();
            string query = "SELECT Id_Periodo, Nombre, Descripcion, Estado FROM dbo.Periodo";
            if (soloActivos)
            {
                query += " WHERE Estado = 1"; // Filtra por activos si se solicita
            }
            query += " ORDER BY Nombre"; // Ordenar alfabéticamente por nombre

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
                                    Estado = Convert.ToBoolean(reader["Estado"])
                                };
                                listaPeriodos.Add(periodo);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerPeriodos: " + ex.Message);
                        // Considera lanzar la excepción o manejarla de otra forma
                        // throw;
                    }
                }
            }
            return listaPeriodos;
        }

        // Método para insertar un nuevo periodo
        public bool InsertarPeriodo(string nombre, string descripcion, bool estado)
        {
            bool insertado = false;
            string query = @"INSERT INTO dbo.Periodo (Nombre, Descripcion, Estado) 
                         VALUES (@Nombre, @Descripcion, @Estado)";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    // Usar parámetros para prevenir SQL Injection
                    cmd.Parameters.AddWithValue("@Nombre", nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", estado);

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
                        // Considera lanzar la excepción o manejarla de otra forma
                    }
                }
            }
            return insertado;
        }

        // Método para actualizar un periodo existente
        public bool ActualizarPeriodo(int Id_Periodo, string nombre, string descripcion, bool estado)
        {
            bool actualizado = false;
            string query = @"UPDATE dbo.Periodo 
                         SET Nombre = @Nombre, Descripcion = @Descripcion, Estado = @Estado 
                         WHERE Id_Periodo = @Id_Periodo";

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", estado);
                    cmd.Parameters.AddWithValue("@Id_Periodo", Id_Periodo);

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
                        // Considera lanzar la excepción o manejarla de otra forma
                    }
                }
            }
            return actualizado;
        }

        // Método para eliminar lógicamente un periodo (cambiar Estado a false)
        public bool EliminarPeriodoLogico(int Id_Periodo)
        {
            bool eliminado = false;
            // Solo actualiza el estado a 0 (inactivo)
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
                        // Considera lanzar la excepción o manejarla de otra forma
                    }
                }
            }
            return eliminado;
        }

        // Método para obtener un periodo específico por ID (útil para la edición)
        public PeriodoInfo ObtenerPeriodoPorId(int Id_Periodo)
        {
            PeriodoInfo periodo = null;
            string query = "SELECT Id_Periodo, Nombre, Descripcion, Estado FROM dbo.Periodo WHERE Id_Periodo = @Id_Periodo";

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
                            if (reader.Read()) // Si se encontró el periodo
                            {
                                periodo = new PeriodoInfo
                                {
                                    Id_Periodo = Convert.ToInt32(reader["Id_Periodo"]),
                                    Nombre = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString() : string.Empty,
                                    Descripcion = reader["Descripcion"] != DBNull.Value ? reader["Descripcion"].ToString() : string.Empty,
                                    Estado = Convert.ToBoolean(reader["Estado"])
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error en ObtenerPeriodoPorId: " + ex.Message);
                        // Considera lanzar la excepción o manejarla de otra forma
                    }
                }
            }
            return periodo;
        }
    }

}