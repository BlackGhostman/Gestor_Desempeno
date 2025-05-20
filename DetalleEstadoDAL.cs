using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

// Namespace from user's code
namespace Gestor_Desempeno
{


    
        public class DetalleEstadoInfo
        {
            // Using property name from user's class
            public int Id_Detalle_Estado { get; set; }
            public int? IdClase { get; set; } // Nullable int
            public string Descripcion { get; set; } // nvarchar(50)
                                                    // Propiedad adicional para mostrar NombreClase
            public string NombreClase { get; set; }
        }


        public class DetalleEstadoDAL
        {
            private string GetConnectionString()
            {
                return ConfigurationManager.ConnectionStrings["ObjetivosConnection"].ConnectionString;
            }

            // Obtener estados, opcionalmente filtrando por clase, incluyendo NombreClase
            // FIXED: Correctly uses parameterized query to prevent SQL Injection
            public List<DetalleEstadoInfo> ObtenerDetallesEstado(int? idClaseFiltro = null)
            {
                List<DetalleEstadoInfo> lista = new List<DetalleEstadoInfo>();
                // Query con LEFT JOIN para obtener NombreClase (incluso si Id_Clase es NULL)
                string query = @"SELECT de.Id_Detalle_Estado, de.Id_Clase, de.Descripcion, c.Nombre AS NombreClase
                             FROM dbo.Detalle_Estado de
                             LEFT JOIN dbo.Clase c ON de.Id_Clase = c.Id_Clase";

                // Añadir filtro si se proporciona IdClase
                if (idClaseFiltro.HasValue && idClaseFiltro.Value > 0) // Asegurar que el filtro sea válido
                {
                    // Use parameter name in the WHERE clause
                    query += " WHERE de.Id_Clase = @IdClaseFiltro";
                }
                query += " ORDER BY c.Nombre, de.Descripcion"; // Ordenar por clase, luego descripción

                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Add parameter ONLY if the filter is applied and used in the query
                        if (idClaseFiltro.HasValue && idClaseFiltro.Value > 0)
                        {
                            // Use the correct parameter name as defined in the WHERE clause
                            cmd.Parameters.AddWithValue("@IdClaseFiltro", idClaseFiltro.Value);
                        }

                        try
                        {
                            con.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    lista.Add(new DetalleEstadoInfo
                                    {
                                        // Use property name from user's class
                                        Id_Detalle_Estado = Convert.ToInt32(reader["Id_Detalle_Estado"]),
                                        IdClase = reader["Id_Clase"] != DBNull.Value ? Convert.ToInt32(reader["Id_Clase"]) : (int?)null,
                                        Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                                        NombreClase = reader["NombreClase"]?.ToString() ?? "N/A" // Mostrar N/A si Id_Clase es NULL
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error en ObtenerDetallesEstado: " + ex.Message);
                            throw; // Re-lanzar
                        }
                    }
                }
                return lista;
            }

            // Método para insertar un nuevo detalle de estado
            public int InsertarDetalleEstado(int? idClase, string descripcion)
            {
                int nuevoId = -1;
                if (string.IsNullOrWhiteSpace(descripcion)) return -1; // Validación

                // ** ADDED: Check for duplicates before inserting **
                if (ExisteDetalleEstado(descripcion, idClase))
                {
                    Console.WriteLine($"Error InsertarDetalleEstado: Ya existe un estado '{descripcion}' para la clase especificada.");
                    throw new InvalidOperationException($"Ya existe un estado con la descripción '{descripcion}' para la clase especificada.");
                }


                string query = @"INSERT INTO dbo.Detalle_Estado (Id_Clase, Descripcion)
                             VALUES (@IdClase, @Descripcion);
                             SELECT SCOPE_IDENTITY();";

                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IdClase", idClase.HasValue ? (object)idClase.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Descripcion", descripcion.Trim());

                        try
                        {
                            con.Open();
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                nuevoId = Convert.ToInt32(result);
                            }
                        }
                        catch (SqlException ex)
                        {
                            Console.WriteLine("Error SQL en InsertarDetalleEstado: " + ex.Message);
                            // Podría haber unique constraints (Descripcion + Id_Clase)
                            throw; // Re-throw
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error general en InsertarDetalleEstado: " + ex.Message);
                            throw; // Re-throw
                        }
                    }
                }
                return nuevoId;
            }

            // Método para actualizar un detalle de estado existente
            // Using Id_Detalle_Estado parameter name to match user's code style
            public bool ActualizarDetalleEstado(int Id_Detalle_Estado, int? idClase, string descripcion)
            {
                bool actualizado = false;
                if (Id_Detalle_Estado <= 0 || string.IsNullOrWhiteSpace(descripcion)) return false; // Validación

                // ** ADDED: Check for duplicates before updating **
                if (ExisteDetalleEstado(descripcion, idClase, Id_Detalle_Estado))
                {
                    Console.WriteLine($"Error ActualizarDetalleEstado: Ya existe otro estado '{descripcion}' para la clase especificada.");
                    throw new InvalidOperationException($"Ya existe otro estado con la descripción '{descripcion}' para la clase especificada.");
                }


                string query = @"UPDATE dbo.Detalle_Estado SET
                                Id_Clase = @IdClase,
                                Descripcion = @Descripcion
                             WHERE Id_Detalle_Estado = @Id_Detalle_Estado"; // Parameter name matches

                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Parameter name matches the one in the query
                        cmd.Parameters.AddWithValue("@Id_Detalle_Estado", Id_Detalle_Estado);
                        cmd.Parameters.AddWithValue("@IdClase", idClase.HasValue ? (object)idClase.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Descripcion", descripcion.Trim());

                        try
                        {
                            con.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();
                            actualizado = (rowsAffected > 0);
                        }
                        catch (SqlException ex)
                        {
                            Console.WriteLine("Error SQL en ActualizarDetalleEstado: " + ex.Message);
                            throw; // Re-throw
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error general en ActualizarDetalleEstado: " + ex.Message);
                            throw; // Re-throw
                        }
                    }
                }
                return actualizado;
            }

            // Método para eliminar FÍSICAMENTE un detalle de estado
            // Using Id_Detalle_Estado parameter name to match user's code style
            public bool EliminarDetalleEstado(int Id_Detalle_Estado)
            {
                if (Id_Detalle_Estado <= 0) return false; // Validación
                bool eliminado = false;
                // Parameter name matches the one used in AddWithValue
                string query = "DELETE FROM dbo.Detalle_Estado WHERE Id_Detalle_Estado = @Id_Detalle_Estado";

                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Parameter name matches the one in the query
                        cmd.Parameters.AddWithValue("@Id_Detalle_Estado", Id_Detalle_Estado);

                        try
                        {
                            con.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();
                            eliminado = (rowsAffected > 0);
                        }
                        catch (SqlException ex)
                        {
                            // Error común: Violación de FK si el estado está en uso
                            if (ex.Number == 547)
                            {
                                Console.WriteLine($"Error al eliminar Detalle_Estado (ID: {Id_Detalle_Estado}): El estado está siendo utilizado por otros registros (Objetivos, Metas, etc.).");
                                throw new InvalidOperationException("No se puede eliminar el estado porque está en uso.", ex);
                            }
                            else
                            {
                                Console.WriteLine($"Error SQL al eliminar Detalle_Estado: {ex.Message}");
                                throw; // Re-lanzar otras excepciones SQL
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error general al eliminar Detalle_Estado: {ex.Message}");
                            throw;
                        }
                    }
                }
                return eliminado;
            }


            // Método para obtener un detalle de estado específico por ID
            // Using Id_Detalle_Estado parameter name to match user's code style
            public DetalleEstadoInfo ObtenerDetalleEstadoPorId(int Id_Detalle_Estado)
            {
                DetalleEstadoInfo detalle = null;
                // Parameter name matches the one used in AddWithValue
                string query = @"SELECT de.Id_Detalle_Estado, de.Id_Clase, de.Descripcion, c.Nombre AS NombreClase
                             FROM dbo.Detalle_Estado de
                             LEFT JOIN dbo.Clase c ON de.Id_Clase = c.Id_Clase
                             WHERE de.Id_Detalle_Estado = @Id_Detalle_Estado";

                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Parameter name matches the one in the query
                        cmd.Parameters.AddWithValue("@Id_Detalle_Estado", Id_Detalle_Estado);
                        try
                        {
                            con.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read()) // Si se encontró
                                {
                                    detalle = new DetalleEstadoInfo
                                    {
                                        // Use property name from user's class
                                        Id_Detalle_Estado = Convert.ToInt32(reader["Id_Detalle_Estado"]),
                                        IdClase = reader["Id_Clase"] != DBNull.Value ? Convert.ToInt32(reader["Id_Clase"]) : (int?)null,
                                        Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                                        NombreClase = reader["NombreClase"]?.ToString() ?? "N/A"
                                    };
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error en ObtenerDetalleEstadoPorId: " + ex.Message);
                        }
                    }
                }
                return detalle;
            }

            // Método para verificar si ya existe un estado para evitar duplicados (misma descripción y clase)
            // ** UPDATED: Use UPPER for case-insensitive comparison **
            public bool ExisteDetalleEstado(string descripcion, int? idClase, int idExcluir = 0)
            {
                if (string.IsNullOrWhiteSpace(descripcion)) return false; // Cannot check empty description

                bool existe = false;
                // Use UPPER for case-insensitive comparison
                string query = @"SELECT COUNT(*) FROM dbo.Detalle_Estado
                              WHERE UPPER(Descripcion) = UPPER(@Descripcion)";
                // Añadir condición para Id_Clase (manejar NULL)
                if (idClase.HasValue)
                {
                    query += " AND Id_Clase = @IdClase";
                }
                else
                {
                    query += " AND Id_Clase IS NULL";
                }

                // Excluir el registro actual si estamos actualizando
                if (idExcluir > 0)
                {
                    // Use parameter name consistent with property/parameter name
                    query += " AND Id_Detalle_Estado <> @IdExcluir";
                }

                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Descripcion", descripcion.Trim()); // Trim before comparison
                        if (idClase.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@IdClase", idClase.Value);
                        }
                        if (idExcluir > 0)
                        {
                            // Use parameter name consistent with query
                            cmd.Parameters.AddWithValue("@IdExcluir", idExcluir);
                        }

                        try
                        {
                            con.Open();
                            int count = (int)cmd.ExecuteScalar();
                            existe = (count > 0);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error en ExisteDetalleEstado: " + ex.Message);
                            // Considerar lanzar la excepción
                            throw;
                        }
                    }
                }
                return existe;
            }

            // Método para obtener ID por descripción (puede ser útil)
            // ** UPDATED: Use UPPER for case-insensitive comparison **
            public int? ObtenerIdEstadoPorDescripcion(string descripcionEstado, int? idClaseFiltro = null)
            {
                if (string.IsNullOrWhiteSpace(descripcionEstado)) return null;

                int? idEstado = null;
                // Use UPPER for case-insensitive comparison
                string query = "SELECT Id_Detalle_Estado FROM dbo.Detalle_Estado WHERE UPPER(Descripcion) = UPPER(@Descripcion)";
                if (idClaseFiltro.HasValue)
                {
                    query += " AND Id_Clase = @IdClase";
                }

                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Descripcion", descripcionEstado.Trim());
                        if (idClaseFiltro.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@IdClase", idClaseFiltro.Value);
                        }
                        try
                        {
                            con.Open();
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                idEstado = Convert.ToInt32(result);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error en ObtenerIdEstadoPorDescripcion: " + ex.Message);
                            // Consider throwing exception
                        }
                    }
                }
                return idEstado;
            }
        }
    } // Closing namespace bracket


