using GestionRecetas.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;


namespace API_SAP.Clases
{
    public class SQLServerManager
    {
        private readonly string connectionString;

        public SQLServerManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<JArray> ObtenerListadoString(string query)
        {
            JArray listado = new JArray("--");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = null;

                try
                {
                    reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        string valor = reader.GetString(0); // Obtener el valor

                        // Verificar si el valor ya está en el listado
                        bool existe = listado.Any(item => item.Type == JTokenType.String && (string)item == valor);

                        if (!existe)
                        {
                            listado.Add(valor);
                        }
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        await reader.CloseAsync();
                    }
                }
            }

            return listado;
        }

        public async Task<JArray> ObtenerListadoInt(string query)
        {
            JArray listado = new JArray(0);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = null;

                try
                {
                    reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        int valor = reader.GetInt32(0); // Obtener el valor

                        // Verificar si el valor ya está en el listado
                        bool existe = listado.Any(item => item.Type == JTokenType.Integer && (int)item == valor);

                        if (!existe)
                        {
                            listado.Add(valor);
                        }
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        await reader.CloseAsync();
                    }
                }
            }

            return listado;
        }

        public async Task<string> ObtenerValor(string query)
        {
            string valor = "";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = null;

                try
                {
                    reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        valor = reader.GetString(0); // Obtener el valor
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        await reader.CloseAsync();
                    }
                }
            }

            return valor;
        }

        public async Task<string> OrdenesFabricacion(int OF)
        {
            string Estado = "Liberada";
            string queryComprobar = "SELECT COUNT(*) FROM OFs WHERE ordenFabricacion = @OF";
            string queryInsertar = "INSERT INTO OFs (ordenFabricacion, status) VALUES (@OF, @Estado)";
            string queryComprobarEstado = "SELECT status FROM OFs WHERE ordenFabricacion = @OF";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Comprobar si la orden de fabricación existe
                SqlCommand commandComprobar = new SqlCommand(queryComprobar, connection);
                commandComprobar.Parameters.AddWithValue("@OF", OF);

                int count = (int)await commandComprobar.ExecuteScalarAsync();

                if (count == 0)
                {
                    // Insertar una nueva orden de fabricación
                    SqlCommand commandInsertar = new SqlCommand(queryInsertar, connection);
                    commandInsertar.Parameters.AddWithValue("@OF", OF);
                    commandInsertar.Parameters.AddWithValue("@Estado", Estado);

                    await commandInsertar.ExecuteNonQueryAsync();
                }
                else
                {
                    SqlCommand command = new SqlCommand(queryComprobarEstado, connection);
                    command.Parameters.AddWithValue("@OF", OF);

                    // Ejecutar la consulta para obtener el estado
                    string estado = await command.ExecuteScalarAsync() as string;

                    if (!string.IsNullOrEmpty(estado))
                    {
                        Estado = estado;
                    }
                }
            }
            return Estado;
        }

        public async Task ActualizarEstado(JObject Datos)
        {
            string estado = Datos["GMDix"]["estado"].ToString();
            string descripcion = Datos["MAKTX"].ToString();
            string receta = Datos["GMDix"]["nombreReceta"].ToString();
            int version = Convert.ToInt32(Datos["GMDix"]["version"]);
            string destino = Datos["GMDix"]["nombreReactor"].ToString();
            string nombreEtapa = "Nombreprueba";
            var itemsComponentes = Datos["COMPONENTES"]["item"];

            int ordenFabricacion = Convert.ToInt32(Datos["AUFNR"]);

            string queryUpdate = @$"UPDATE OFs 
                              SET status = '{estado}',
                                  fechaInicio = GETDATE(),
                                  descripcion = '{descripcion}',
                                  nombreReceta = '{receta}',
                                  version = {version},
                                  nombreReactor = '{destino}',
                                  nombreEtapa = '{nombreEtapa}',
                                  numeroEtapa = '0/' + CAST((SELECT numeroEtapas FROM Recetas WHERE nombreReceta LIKE '{receta}' AND version LIKE {version}) AS NVARCHAR(MAX))
                              WHERE ordenFabricacion = {ordenFabricacion}";

            if (itemsComponentes is JArray itemsArray)
            {
                foreach (var item in itemsArray)
                {
                    var materiaPrima = item["MAKTX"].ToString();
                    var cantidad = item["BDMNG"];
                    var ud = item["MEINS"].ToString();

                    string queryInsert = @$"
                                INSERT INTO Materiales (ordenFabricacion, materiaPrima, cantidad, ud, fechaLanzada)
                                VALUES ({ordenFabricacion}, '{materiaPrima}', '{cantidad}', '{ud}', GETDATE())";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                                               
                        using (SqlCommand commandInsertar = new SqlCommand(queryInsert, connection))
                        {
                            await commandInsertar.ExecuteNonQueryAsync();
                        }

                        await connection.CloseAsync();
                    }
                }
            }
            else
            {
                var materiaPrima = itemsComponentes["MAKTX"].ToString();
                var cantidad = itemsComponentes["BDMNG"];
                var ud = itemsComponentes["MEINS"].ToString();

                string queryInsert = @$"
                                INSERT INTO Materiales (ordenFabricacion, materiaPrima, cantidad, ud, fechaLanzada)
                                VALUES ({ordenFabricacion}, '{materiaPrima}', '{cantidad}', '{ud}', GETDATE())";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand commandInsertar = new SqlCommand(queryInsert, connection))
                    {
                        await commandInsertar.ExecuteNonQueryAsync();
                    }

                    await connection.CloseAsync();
                }
            }
            

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Actualizar el estado y la fecha de inicio de la orden de fabricación
                using (SqlCommand commandActualizar = new SqlCommand(queryUpdate, connection))
                {
                    await commandActualizar.ExecuteNonQueryAsync();
                }

                await connection.CloseAsync();
            }
        }

        public async Task<List<JObject>> ListadoLanzadas()
        {
            List<JObject> jsonObjectList = new List<JObject>();

            //string query = "SELECT FechaInicio, OrdenFabricacion AS OF, Descripcion, Receta, Version, Destino, NombreEtapa AS Etapa, NumeroEtapa, Status AS Estado FROM OFs WHERE Status != 'Liberada'";
            string query = "SELECT * FROM OFs WHERE status = 'Lanzada'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        if ((string?)reader["status"] == "Lanzada")
                        {
                            JObject jsonObject = new JObject();
                            jsonObject["fechaInicio"] = (DateTime?)(reader["fechaInicio"] is DBNull ? null : reader["fechaInicio"]);
                            jsonObject["OF"] = (int?)reader["ordenFabricacion"];
                            jsonObject["descripcion"] = (string?)(reader["descripcion"] is DBNull ? null : reader["descripcion"]);
                            jsonObject["nombreReceta"] = (string?)(reader["nombreReceta"] is DBNull ? null : reader["nombreReceta"]);
                            jsonObject["version"] = (int?)(reader["version"] is DBNull ? null : reader["version"]);
                            jsonObject["nombreReactor"] = (string?)(reader["nombreReactor"] is DBNull ? null : reader["nombreReactor"]);
                            jsonObject["nombreEtapa"] = (string?)(reader["nombreEtapa"] is DBNull ? null : reader["nombreEtapa"]);
                            jsonObject["numeroEtapa"] = (string?)(reader["numeroEtapa"] is DBNull ? null : reader["numeroEtapa"]);
                            jsonObject["estado"] = (string?)reader["status"];

                            jsonObjectList.Add(jsonObject);
                        }

                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    // Maneja la excepción adecuadamente, según tus necesidades
                    Console.WriteLine($"Error al ejecutar la consulta: {ex.Message}");
                }

                await connection.CloseAsync();
            }

            return jsonObjectList;
        }

        public async Task<object> RevisarLazadas(string nombreReactor)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("ObtenerRecetaLanzada", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@nombreReactor", nombreReactor);

                    try
                    {
                        SqlDataReader reader = await command.ExecuteReaderAsync();

                        List<Dictionary<string, object>> recetas = new List<Dictionary<string, object>>();

                        while (await reader.ReadAsync())
                        {
                            Dictionary<string, object> receta = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                receta[reader.GetName(i)] = reader.GetValue(i);
                            }

                            recetas.Add(receta);
                        }

                        reader.Close();

                        if (recetas.Count == 0)
                        {
                            return false; // Si no hay recetas
                        }

                        return recetas;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public async Task ActualizarOF(string OF, string nombreEtapa, string numeroEtapa)
        {
            string query = @"UPDATE OFs
                            SET nombreEtapa = @nombreEtapa,
                                numeroEtapa = @numeroEtapa
                            WHERE ordenFabricacion = @OF";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@nombreEtapa", nombreEtapa);
                    command.Parameters.AddWithValue("@numeroEtapa", numeroEtapa);
                    command.Parameters.AddWithValue("@OF", OF);

                    int filasAfectadas = await command.ExecuteNonQueryAsync();

                    if (filasAfectadas == 0)
                    {
                        // No se encontró la OF
                        Console.WriteLine("No se actualizó ninguna fila. Verifica que la OF exista.");
                    }
                    else
                    {
                        Console.WriteLine("Actualización de etapa exitosa.");
                    }
                }
            }
        }

        public async Task FinalizarOF(string OF, string estado)
        {
            string query = @"UPDATE OFs
                            SET status = @estado,
                            fechaFin = GETDATE()
                            WHERE ordenFabricacion = @OF";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OF", OF);
                    command.Parameters.AddWithValue("@estado", estado);

                    int filasAfectadas = await command.ExecuteNonQueryAsync();

                    if (filasAfectadas == 0)
                    {
                        // No se encontró la OF
                        Console.WriteLine("No se actualizó el estado. Verifica que la OF exista.");
                    }
                    else
                    {
                        Console.WriteLine("Actualización de status de OF exitosa.");
                    }
                }
            }
        }

        #region Basquevolt
        //Metodos de basquevolt
        public List<T> GetDatos<T>(string query)
        {
            List<T> Filas = new List<T>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    T Fila = Activator.CreateInstance<T>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);
                        object columnValue = reader.GetValue(i);

                        PropertyInfo property = typeof(T).GetProperty(columnName);

                        if (property != null && columnValue != DBNull.Value)
                        {
                            property.SetValue(Fila, columnValue);
                        }
                    }

                    Filas.Add(Fila);
                }
                reader.Close();
                connection.Close();
            }

            return Filas;
        }

        public List<T> GetTabla<T>(string tableName)
        {
            List<T> Filas = new List<T>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT * FROM {tableName}";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    T Fila = Activator.CreateInstance<T>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);
                        object columnValue = reader.GetValue(i);

                        PropertyInfo property = typeof(T).GetProperty(columnName);

                        if (property != null && columnValue != DBNull.Value)
                        {
                            if (columnName == "ID_Reactor")
                            {
                                property.SetValue(Fila, columnValue.ToString());
                            }
                            else
                            {
                                property.SetValue(Fila, columnValue);
                            }

                        }
                    }

                    Filas.Add(Fila);
                }

                reader.Close();
            }

            return Filas;
        }

        public List<Recetas> GetRecetas(string TablaRecetas)
        {
            List<Recetas> ListadoRecetas = new List<Recetas>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT R.ID, T2.Nombre AS ID_Reactor, R.Nombre, R.Bloqueada, R.Creada, R.Modificada, R.Eliminada FROM {TablaRecetas} R JOIN Reactores T2 ON R.ID_Reactor = T2.ID";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Recetas Fila = Activator.CreateInstance<Recetas>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);
                        object columnValue = reader.GetValue(i);

                        PropertyInfo property = typeof(Recetas).GetProperty(columnName);

                        if (property != null && columnValue != DBNull.Value)
                        {
                            property.SetValue(Fila, columnValue);
                        }
                    }
                    ListadoRecetas.Add(Fila);
                }
                reader.Close();
                connection.Close();
            }

            return ListadoRecetas;
        }
        #endregion
    }
}