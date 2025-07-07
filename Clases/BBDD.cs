using GestionRecetas.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
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
            string nombreEtapa = "Cargando Datos Receta...";
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
                //Console.WriteLine("Connection String: " + connection.ConnectionString);

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
                        //Console.WriteLine("Actualización de etapa exitosa.");
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
                        //Console.WriteLine("Actualización de status de OF exitosa.");
                    }
                }
            }
        }

        public async Task InsertarMaterial(int i_count, string Nombre, string Operacion, string PuestoTrabajo)
        {
            string query_InsertMat = @"INSERT INTO Materias (ID, Nombre, Operacion, PuestoTrabajo)
                               VALUES (@ID, @Nombre, @Operacion, @PuestoTrabajo)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query_InsertMat, connection))
                {
                    // Agregamos los parámetros reales que sí están en la consulta
                    command.Parameters.AddWithValue("@ID", i_count);
                    command.Parameters.AddWithValue("@Nombre", Nombre);
                    command.Parameters.AddWithValue("@Operacion", Operacion);
                    command.Parameters.AddWithValue("@PuestoTrabajo", PuestoTrabajo);

                    int filasAfectadas = await command.ExecuteNonQueryAsync();

                    if (filasAfectadas == 0)
                    {
                        Console.WriteLine("No se insertó ningún material.");
                    }
                    else
                    {
                        //Console.WriteLine("Inserción de material exitosa.");
                    }
                }
            }
        }

        public async Task<bool> ExisteMaterial(string material)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT COUNT(*) FROM Materias WHERE Nombre = @material";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@material", material);

                int count = (int)await command.ExecuteScalarAsync();

                return count > 0;
            }
        }

        public async Task EliminarTodosLosMateriales()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "DELETE FROM Materias"; // o TRUNCATE TABLE Materias si no hay FK
                SqlCommand cmd = new SqlCommand(query, connection);
                await cmd.ExecuteNonQueryAsync();
            }
        }


        public async Task<decimal?> ExtraerValorMMPP(int? ID_Receta, int N_Etapa, decimal MMPP)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                //Console.WriteLine($"Conexión abierta. Buscando MateriaPrima para ID_Receta={ID_Receta}, N_Etapa={N_Etapa}, MMPP={MMPP}");
                // Verificamos si existe la fila con MateriaPrima
                string query_found = @"SELECT 1
                               FROM ProcesoPrincipal
                               WHERE ID_Receta = @ID_Receta
                                 AND N_Etapa = @N_Etapa
                                 AND Consigna = 'MateriaPrima'
                                 AND Valor = @MMPP;";

                //Console.WriteLine($"Query de seleccion: {query_found}");

                using (SqlCommand checkCommand = new SqlCommand(query_found, connection))
                {
                    checkCommand.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                    checkCommand.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                    checkCommand.Parameters.AddWithValue("@MMPP", MMPP);

                    var exists = await checkCommand.ExecuteScalarAsync();

                    if (exists != null)
                    {
                        //Console.WriteLine("MateriaPrima encontrada. Buscando valor siguiente (Cantidad)...");
                        // Si existe, obtenemos el valor de la siguiente fila (Cantidad)
                        string query = @"SELECT siguiente.Valor
                                 FROM ProcesoPrincipal actual
                                 JOIN ProcesoPrincipal siguiente ON siguiente.ID = actual.ID + 1
                                 WHERE actual.Consigna = 'MateriaPrima'
                                   AND actual.Valor = @MMPP
                                   AND siguiente.Consigna = 'Cantidad'
                                   AND actual.ID_Receta = @ID_Receta
                                   AND actual.N_Etapa = @N_Etapa;";

                        //Console.WriteLine($"Query de Inserccion: {query}");

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                            command.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                            command.Parameters.AddWithValue("@MMPP", MMPP);

                            var result = await command.ExecuteScalarAsync();

                            if (result != null && decimal.TryParse(result.ToString(), out decimal valorCantidad))
                            {
                                //Console.WriteLine($"Valor Cantidad encontrado: {valorCantidad}");
                                return valorCantidad;
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("No se encontró Materia Prima.");
                    }

                    return null;
                }
            }
        }

        public async Task<decimal?> ExtraerValorTIEMPO(int? ID_Receta, int N_Etapa)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                //Console.WriteLine($"Conexión abierta. Buscando MateriaPrima para ID_Receta={ID_Receta}, N_Etapa={N_Etapa}");
                // Verificamos si existe la fila con MateriaPrima
                string query_found = @"SELECT 1
                               FROM ProcesoPrincipal
                               WHERE ID_Receta = @ID_Receta
                                 AND N_Etapa = @N_Etapa
                                 AND Consigna = 'Tiempo';
                                 ";

                //Console.WriteLine($"Query de seleccion: {query_found}");

                using (SqlCommand checkCommand = new SqlCommand(query_found, connection))
                {
                    checkCommand.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                    checkCommand.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                    
                    var exists = await checkCommand.ExecuteScalarAsync();

                    if (exists != null)
                    {
                        //Console.WriteLine("MateriaPrima encontrada. Buscando valor siguiente (Cantidad)...");
                        // Si existe, obtenemos el valor de la siguiente fila (Cantidad)
                        string query = @"SELECT valor
                                 FROM ProcesoPrincipal 
                                   WHERE Consigna = 'Tiempo'
                                   AND ID_Receta = @ID_Receta
                                   AND N_Etapa = @N_Etapa;";

                        //Console.WriteLine($"Query de Inserccion: {query}");

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                            command.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                            
                            var result = await command.ExecuteScalarAsync();

                            if (result != null && decimal.TryParse(result.ToString(), out decimal valorCantidad))
                            {
                                //Console.WriteLine($"Valor Cantidad encontrado: {valorCantidad}");
                                return valorCantidad;
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("No se encontró Materia Prima.");
                    }

                    return null;
                }
            }
        }
        
        public async Task<decimal?> ExtraerOperario(int? ID_Receta, int N_Etapa)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                //Console.WriteLine($"🔍 Buscando 'Operador' para ID_Receta={ID_Receta}, N_Etapa={N_Etapa}");

                string query = @"
                                SELECT 1
                                FROM ProcesoPrincipal
                                WHERE ID_Receta = @ID_Receta
                                  AND N_Etapa = @N_Etapa
                                  AND Tipo = 'Operador';
                            ";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                    command.Parameters.AddWithValue("@N_Etapa", N_Etapa);

                    var result = await command.ExecuteScalarAsync();

                    if (result != null && decimal.TryParse(result.ToString(), out decimal valor))
                    {
                        //Console.WriteLine($"✅ Valor encontrado: {valor}");
                        return valor;
                    }
                    else
                    {
                        //Console.WriteLine("⚠️ No se encontró el valor del operador.");
                        return null;
                    }
                }
            }
        }


        public async Task<decimal?> ExtraerAgitacion(int? ID_Receta, int N_Etapa,string Consigna)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                               
                string query = @"
                                SELECT Valor
                                FROM ProcesoAgitacion
                                WHERE ID_Receta = @ID_Receta
                                  AND N_Etapa = @N_Etapa
                                  AND Tipo = 'Agitacion'
                                  AND Consigna = @Consigna;
                            ";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                    command.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                    command.Parameters.AddWithValue("@Consigna", Consigna);

                    var result = await command.ExecuteScalarAsync();

                    if (result != null && decimal.TryParse(result.ToString(), out decimal valor))
                    {
                        //Console.WriteLine($"✅ Valor encontrado: {valor}");
                        return valor;
                    }
                    else
                    {
                        //Console.WriteLine("⚠️ No se encontró el valor del operador.");
                        return null;
                    }
                }
            }
        }

        public async Task<decimal?> ExtraerTemperatura(int? ID_Receta, int N_Etapa, string Consigna)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                                SELECT Valor
                                FROM ProcesoTemperatura
                                WHERE ID_Receta = @ID_Receta
                                  AND N_Etapa = @N_Etapa
                                  AND Tipo = 'Temperatura'
                                  AND Consigna = @Consigna;
                            ";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                    command.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                    command.Parameters.AddWithValue("@Consigna", Consigna);

                    var result = await command.ExecuteScalarAsync();

                    if (result != null && decimal.TryParse(result.ToString(), out decimal valor))
                    {
                        //Console.WriteLine($"✅ Valor encontrado: {valor}");
                        return valor;
                    }
                    else
                    {
                        //Console.WriteLine("⚠️ No se encontró el valor del operador.");
                        return null;
                    }
                }
            }
        }

        public async Task<int?> ObtenerIDReceta(string NombreReceta)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                        SELECT ID
                        FROM Recetas
                        WHERE NombreReceta = @NombreReceta;
                        ";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NombreReceta", NombreReceta);

                    var result = await command.ExecuteScalarAsync();

                    if (result != null && int.TryParse(result.ToString(), out int id))
                    {
                        return id;
                    }
                    else
                    {
                        //Console.WriteLine("⚠️ No se encontró el valor del operador.");
                        return null;
                    }
                }
            }
        }

        public async Task<List<string>> ObtenerOFLanzadas()
        {
            var listaOFs = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                        SELECT DISTINCT OrdenFabricacion
                        FROM OFs
                        WHERE Status = 'Lanzada'";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string orden = reader[0]?.ToString();
                        if (!string.IsNullOrWhiteSpace(orden))
                        {
                            listaOFs.Add(orden);
                        }
                    }
                }
            }

            return listaOFs;
        }

        public async Task<List<Dictionary<string, object>>> ObtenerMateriasPrimas(string query)
        {
            var listaMaterias = new List<Dictionary<string, object>>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var dict = new Dictionary<string, object>
                        {
                            ["materiaPrima"] = reader["materiaPrima"],
                            ["cantidad"] = reader["cantidad"]
                        };
                        listaMaterias.Add(dict);
                    }
                }
            }

            return listaMaterias;
        }

        public async Task<Dictionary<string, string>> ObtenerMateriasPrimasReales(string ordenFabricacion)
        {
            var resultado = new Dictionary<string, string>();

            string query = $@"
                        SELECT solido1, solido2, solido3, Agua, AguaRecu, Antiespumante, Ligno, potasa
                        FROM datosRealesMMPP
                        WHERE ordenFabricacion = @ordenFabricacion";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ordenFabricacion", ordenFabricacion);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            resultado["lc70"] = reader["solido1"].ToString();
                            resultado["lc80"] = reader["solido2"].ToString();
                            resultado["hl26"] = reader["solido3"].ToString();
                            resultado["agua"] = reader["Agua"].ToString();
                            resultado["aguaRecuperada"] = reader["AguaRecu"].ToString();
                            resultado["antiespumante"] = reader["Antiespumante"].ToString();
                            resultado["ligno"] = reader["Ligno"].ToString();
                            resultado["potasa"] = reader["potasa"].ToString();
                        }
                    }
                }
            }

            return resultado;
        }

        public async Task<string> ExtraerNombreEtapa(int? ID_Receta, int ID_Etapa)
        {
            string Nombre_Etapa = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                                SELECT Nombre
                                FROM Etapas
                                WHERE N_Etapa = @ID_Etapa
                                  AND ID_Receta = @ID_Receta";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID_Etapa", ID_Etapa);
                    command.Parameters.AddWithValue("@ID_Receta", ID_Receta);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Nombre_Etapa = reader["Nombre"]?.ToString();
                        }
                    }
                }
            }

            return Nombre_Etapa;
        }

        public async Task<Models.MMPPFinal> ObtenerMMPPFinales(string OF)
        {
            Models.MMPPFinal resultado = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                            SELECT TOP 1 *
                            FROM MMPP_Finales
                            WHERE OrdenFabricacion = @OF";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OF", OF);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            resultado = new Models.MMPPFinal
                            {
                                OrdenFabricacion = reader["OrdenFabricacion"].ToString(),
                                FechaInsercion = Convert.ToDateTime(reader["FechaInsercion"]),
                                Solidos_1_CT = Convert.ToInt32(reader["Solidos_1_CT"]),
                                Solidos_1_CR = Convert.ToInt32(reader["Solidos_1_CR"]),
                                Solidos_2_CT = Convert.ToInt32(reader["Solidos_2_CT"]),
                                Solidos_2_CR = Convert.ToInt32(reader["Solidos_2_CR"]),
                                Solidos_3_CT = Convert.ToInt32(reader["Solidos_3_CT"]),
                                Solidos_3_CR = Convert.ToInt32(reader["Solidos_3_CR"]),
                                Agua_CT = Convert.ToInt32(reader["Agua_CT"]),
                                Agua_CR = Convert.ToInt32(reader["Agua_CR"]),
                                Agua_Recu_CT = Convert.ToInt32(reader["Agua_Recu_CT"]),
                                Agua_Recu_CR = Convert.ToInt32(reader["Agua_Recu_CR"]),
                                Antiespumante_CT = Convert.ToInt32(reader["Antiespumante_CT"]),
                                Antiespumante_CR = Convert.ToInt32(reader["Antiespumante_CR"]),
                                Ligno_CT = Convert.ToInt32(reader["Ligno_CT"]),
                                Ligno_CR = Convert.ToInt32(reader["Ligno_CR"]),
                                Potasa_CT = Convert.ToInt32(reader["Potasa_CT"]),
                                Potasa_CR = Convert.ToInt32(reader["Potasa_CR"])
                            };
                        }
                    }
                }
            }

            return resultado;
        }

        public async Task<string> ExtraerOFFinalizadas()
        {
            List<string> ordenes = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"SELECT OrdenFabricacion FROM MMPP_Finales;";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string of = reader["OrdenFabricacion"].ToString();
                        ordenes.Add(of);
                    }
                }
            }

            // Serializar a JSON (necesitas using Newtonsoft.Json;)
            return JsonConvert.SerializeObject(ordenes);
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