using GestionRecetas.Clases;
using GestionRecetas.Models;
using Humanizer;
using Microsoft.CodeAnalysis.RulesetToEditorconfig;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Reflection;


namespace API_SAP.Clases
{
    public class SQLServerManager
    {
        // ---------------------------------------------------------------------------------------------------------------------------

        private readonly string connectionString;

        // ---------------------------------------------------------------------------------------------------------------------------

        public SQLServerManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Método asíncrono que devuelve un arreglo JSON (JArray) de cadenas únicas (sin duplicados).
        // Recibe una consulta SQL como parámetro.

        public async Task<JArray> ObtenerListadoString(string query)
        {
            // Creamos un arreglo JSON y le agregamos un valor inicial "--".
            // Esto puede ser un valor por defecto o separador visible (opcional, depende del contexto).

            JArray listado = new JArray("--");

            // Creamos una conexión con la base de datos usando la cadena de conexión existente.
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrimos la conexión a la base de datos de forma asíncrona (sin bloquear el programa principal).
                await connection.OpenAsync();

                // Creamos un comando SQL con la consulta recibida.
                SqlCommand command = new SqlCommand(query, connection);
                // Inicializamos un lector de datos para recorrer los resultados de la consulta.
                SqlDataReader reader = null;

                try
                {
                    // Ejecutamos la consulta y obtenemos los datos en forma de un lector.
                    reader = await command.ExecuteReaderAsync();

                    // Mientras haya filas por leer en el resultado de la consulta...
                    while (await reader.ReadAsync())
                    {
                        // Obtenemos el primer valor (columna 0) de la fila como string.
                        string valor = reader.GetString(0);

                        // Verificamos si ese valor ya está en el listado (para evitar duplicados).
                        bool existe = listado.Any(item => item.Type == JTokenType.String && (string)item == valor);

                        if (!existe)
                        {
                            // Si el valor no existe aún, lo agregamos al JArray.
                            listado.Add(valor);
                        }
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        // Cerramos el lector al terminar, en un bloque "finally" para asegurarnos de que siempre se cierre.
                        await reader.CloseAsync();
                    }
                }
            }
            // Devolvemos el arreglo final con los strings únicos obtenidos desde la base de datos.
            return listado;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Esta función es asíncrona y devuelve un JArray (un arreglo de JSON).
        // Recibe un string que contiene una consulta SQL que se ejecutará en la base de datos.

        public async Task<JArray> ObtenerListadoInt(string query)
        {
            // Creamos un arreglo JSON llamado `listado` que almacenará números enteros únicos.
            JArray listado = new JArray(0);

            // Establecemos una conexión a la base de datos usando la cadena de conexión definida.
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrimos la conexión de forma asíncrona.
                await connection.OpenAsync();

                // Creamos el comando SQL usando la consulta recibida y la conexión abierta.
                SqlCommand command = new SqlCommand(query, connection);
                // Inicializamos el lector de datos que leerá los resultados de la consulta.
                SqlDataReader reader = null;

                try
                {
                    // Ejecutamos el comando y obtenemos un lector para recorrer los resultados.
                    reader = await command.ExecuteReaderAsync();

                    // Mientras haya filas disponibles, seguimos leyendo.
                    while (await reader.ReadAsync())
                    {
                        // Obtenemos el primer valor (columna 0) de la fila actual como entero.
                        int valor = reader.GetInt32(0);

                        // Verificamos si ese valor ya está en el listado.
                        bool existe = listado.Any(item => item.Type == JTokenType.Integer && (int)item == valor);

                        if (!existe)
                        {
                            // Si no existe, lo agregamos al JArray para evitar duplicados.
                            listado.Add(valor);
                        }
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        // Cerramos el lector una vez que terminamos de leer, en un bloque seguro.
                        await reader.CloseAsync();
                    }
                }
            }
            // Devolvemos el listado final con los enteros únicos encontrados en la consulta.
            return listado;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Método asíncrono que devuelve un valor de tipo texto (string)
        // Se espera que la consulta SQL (query) devuelva una sola columna y al menos una fila.

        public async Task<string> ObtenerValor(string query)
        {
            // Creamos una variable para guardar el resultado obtenido de la base de datos.
            // Empieza vacía.
            string valor = "";

            // Creamos y abrimos una conexión con la base de datos SQL Server
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrimos la conexión de manera asíncrona (sin bloquear el hilo principal).
                await connection.OpenAsync();

                // Creamos un comando SQL con la consulta recibida y lo enlazamos a la conexión.
                SqlCommand command = new SqlCommand(query, connection);

                // El lector se usa para leer los resultados de la consulta fila por fila.
                SqlDataReader reader = null;

                try
                {
                    // Ejecutamos la consulta y obtenemos el lector de resultados.
                    reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        // Leemos el valor de la primera columna (índice 0) de la fila actual.
                        // Si hay más de una fila, se sobrescribe y se queda con el último valor leído.
                        valor = reader.GetString(0); // Obtener el valor
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        // Cerramos el lector de datos para liberar recursos,
                        // siempre, incluso si ocurre un error (gracias al bloque `finally`).
                        await reader.CloseAsync();
                    }
                }
            }
            // Devolvemos el valor leído (puede ser vacío si no se encontró ningún dato).
            return valor;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Este método se conecta a la base de datos y gestiona órdenes de fabricación (OF).
        // Si la orden no existe, la inserta con estado "Liberada".
        // Si ya existe, devuelve el estado actual desde la base de datos.

        public async Task<string> OrdenesFabricacion(int OF)
        {
            // Por defecto, la orden estará en estado "Liberada" si no existe aún.
            string Estado = "Liberada";
            // Consulta para verificar si ya existe una orden de fabricación con ese número
            string queryComprobar = "SELECT COUNT(*) FROM OFs WHERE ordenFabricacion = @OF";
            // Consulta para insertar una nueva orden de fabricación con estado
            string queryInsertar = "INSERT INTO OFs (ordenFabricacion, status) VALUES (@OF, @Estado)";
            // Consulta para obtener el estado actual de una orden ya existente
            string queryComprobarEstado = "SELECT status FROM OFs WHERE ordenFabricacion = @OF";

            // Abrimos la conexión a la base de datos
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(); // Se abre la conexión de manera asíncrona

                // Creamos el comando para comprobar si la orden ya existe
                SqlCommand commandComprobar = new SqlCommand(queryComprobar, connection);
                // Se reemplaza el parámetro @OF en la consulta con el valor recibido
                commandComprobar.Parameters.AddWithValue("@OF", OF);

                // Ejecutamos la consulta y obtenemos el número de coincidencias encontradas
                int count = (int)await commandComprobar.ExecuteScalarAsync();

                // Si no se encontró ninguna orden con ese número, la insertamos nueva
                if (count == 0)
                {
                    // Insertamos con el estado por defecto: "Liberada"
                    SqlCommand commandInsertar = new SqlCommand(queryInsertar, connection);
                    commandInsertar.Parameters.AddWithValue("@OF", OF);
                    commandInsertar.Parameters.AddWithValue("@Estado", Estado);

                    // Ejecutamos la inserción (no devuelve datos, solo la realiza)
                    await commandInsertar.ExecuteNonQueryAsync();
                }
                else
                {
                    // Si la orden ya existe, consultamos su estado actual
                    SqlCommand command = new SqlCommand(queryComprobarEstado, connection);
                    command.Parameters.AddWithValue("@OF", OF);

                    // Ejecutamos la consulta para leer el estado
                    string estado = await command.ExecuteScalarAsync() as string;

                    if (!string.IsNullOrEmpty(estado))
                    {
                        // Si el estado no es nulo ni vacío, lo usamos como nuevo valor
                        Estado = estado;
                    }
                }
            }
            // Devolvemos el estado final (ya sea el predeterminado o el que estaba guardado)
            return Estado;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Actualiza el estado y detalles de una orden de fabricación en la base de datos.
        // Inserta los materiales usados según los datos recibidos en formato JSON.
        // Realiza operaciones asíncronas para mayor eficiencia y evita bloqueos.

        public async Task ActualizarEstado(JObject Datos)
        {
            // Extraemos varios datos importantes del objeto JSON recibido
            string estado = Datos["GMDix"]["estado"].ToString();            // Estado actual de la orden
            string descripcion = Datos["MAKTX"].ToString();                 // Descripción del producto o receta
            string receta = Datos["GMDix"]["nombreReceta"].ToString();      // Nombre de la receta
            int version = Convert.ToInt32(Datos["GMDix"]["version"]);       // Versión de la receta
            string destino = Datos["GMDix"]["nombreReactor"].ToString();    // Nombre del reactor destino
            string nombreEtapa = "Cargando Datos Receta...";                // Nombre de la etapa actual (fijo aquí)
            var itemsComponentes = Datos["COMPONENTES"]["item"];            // Lista de componentes o materias primas

            int ordenFabricacion = Convert.ToInt32(Datos["AUFNR"]);         // Número de orden de fabricación

            // Creamos una consulta SQL para actualizar el estado y otros datos en la tabla OFs
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

            // Verificamos si "itemsComponentes" es una lista (array) de componentes
            if (itemsComponentes is JArray itemsArray)
            {
                // Si es una lista, recorremos cada componente para insertarlo en la base de datos
                foreach (var item in itemsArray)
                {
                    var materiaPrima = item["MAKTX"].ToString();         // Nombre de la materia prima
                    var cantidad = item["BDMNG"];                        // Cantidad requerida
                    var ud = item["MEINS"].ToString();                   // Unidad de medida

                    // Consulta SQL para insertar el componente en la tabla Materiales
                    string queryInsert = @$"
                                INSERT INTO Materiales (ordenFabricacion, materiaPrima, cantidad, ud, fechaLanzada)
                                VALUES ({ordenFabricacion}, '{materiaPrima}', '{cantidad}', '{ud}', GETDATE())";

                    // Abrimos conexión a la base de datos
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        // Ejecutamos la consulta para insertar el componente              
                        using (SqlCommand commandInsertar = new SqlCommand(queryInsert, connection))
                        {
                            await commandInsertar.ExecuteNonQueryAsync();
                        }
                        // Cerramos la conexión
                        await connection.CloseAsync();
                    }
                }
            }
            else
            {
                // Si no es una lista, significa que solo hay un componente
                var materiaPrima = itemsComponentes["MAKTX"].ToString();
                var cantidad = itemsComponentes["BDMNG"];
                var ud = itemsComponentes["MEINS"].ToString();

                // Consulta SQL para insertar el componente único en la tabla Materiales
                string queryInsert = @$"
                                INSERT INTO Materiales (ordenFabricacion, materiaPrima, cantidad, ud, fechaLanzada)
                                VALUES ({ordenFabricacion}, '{materiaPrima}', '{cantidad}', '{ud}', GETDATE())";

                // Abrimos conexión a la base de datos
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Ejecutamos la consulta para insertar el componente
                    using (SqlCommand commandInsertar = new SqlCommand(queryInsert, connection))
                    {
                        await commandInsertar.ExecuteNonQueryAsync();
                    }
                    // Cerramos la conexión
                    await connection.CloseAsync();
                }
            }
            // Finalmente, abrimos la conexión para actualizar el estado general de la orden
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Ejecutamos la consulta para actualizar la tabla OFs con los datos nuevos
                using (SqlCommand commandActualizar = new SqlCommand(queryUpdate, connection))
                {
                    await commandActualizar.ExecuteNonQueryAsync();
                }
                // Cerramos la conexión
                await connection.CloseAsync();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Obtiene una lista de órdenes de fabricación con estado "Lanzada" desde la base de datos,
        // convierte cada registro en un objeto JSON y los devuelve en una lista.
        // Realiza la consulta y lectura de datos de forma asíncrona para mejor rendimiento.

        public async Task<List<JObject>> ListadoLanzadas()
        {
            // Lista donde se almacenarán los objetos JSON con la información de cada orden
            List<JObject> jsonObjectList = new List<JObject>();

            // Consulta SQL para obtener todas las órdenes con estado 'Lanzada'
            string query = "SELECT * FROM OFs WHERE status = 'Lanzada'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrimos la conexión a la base de datos de manera asíncrona
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    // Ejecutamos la consulta y obtenemos un lector para recorrer los resultados
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    // Leemos cada fila de resultados mientras haya datos
                    while (await reader.ReadAsync())
                    {
                        // Confirmamos que el estado es "Lanzada" (por seguridad)
                        if ((string?)reader["status"] == "Lanzada")
                        {
                            // Creamos un objeto JSON con los campos seleccionados de la fila actual
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

                            // Añadimos el objeto JSON a la lista de resultados
                            jsonObjectList.Add(jsonObject);
                        }
                    }
                    // Cerramos el lector una vez terminado
                    reader.Close();
                }
                catch (Exception ex)
                {
                    // En caso de error, imprimimos un mensaje con la excepción
                    Console.WriteLine($"Error al ejecutar la consulta: {ex.Message}");
                }
                // Cerramos la conexión a la base de datos
                await connection.CloseAsync();
            }
            // Retornamos la lista con todos los objetos JSON encontrados
            return jsonObjectList;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Ejecuta un procedimiento almacenado para obtener recetas lanzadas de un reactor específico.
        // Devuelve una lista de recetas como diccionarios con sus datos, o false si no hay resultados o error.
        // Usa operaciones asíncronas para mejorar el rendimiento y evitar bloqueos.

        public async Task<object> RevisarLazadas(string nombreReactor)
        {
            // Abrimos conexión a la base de datos de forma asíncrona
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Creamos un comando para ejecutar el procedimiento almacenado "ObtenerRecetaLanzada"
                using (SqlCommand command = new SqlCommand("ObtenerRecetaLanzada", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Agregamos el parámetro requerido para el procedimiento
                    command.Parameters.AddWithValue("@nombreReactor", nombreReactor);

                    try
                    {
                        // Ejecutamos el lector para obtener los resultados del procedimiento
                        SqlDataReader reader = await command.ExecuteReaderAsync();

                        // Lista para guardar las recetas obtenidas
                        List<Dictionary<string, object>> recetas = new List<Dictionary<string, object>>();

                        // Leemos cada fila de resultados
                        while (await reader.ReadAsync())
                        {
                            // Creamos un diccionario para almacenar los campos de la fila
                            Dictionary<string, object> receta = new Dictionary<string, object>();

                            // Recorremos todas las columnas de la fila
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                // Guardamos el nombre de la columna y su valor
                                receta[reader.GetName(i)] = reader.GetValue(i);
                            }
                            // Añadimos la receta a la lista
                            recetas.Add(receta);
                        }
                        reader.Close();
                        // Si no se encontraron recetas, retornamos false
                        if (recetas.Count == 0)
                        {
                            return false; // Si no hay recetas
                        }
                        // Retornamos la lista de recetas encontradas
                        return recetas;
                    }
                    catch (Exception ex)
                    {
                        // En caso de error, mostramos el mensaje y retornamos false
                        Console.WriteLine($"Error: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Actualiza la etapa y el número de etapa de una orden de fabricación(OF) en la base de datos.
        // Recibe como parámetros el número de orden, el nombre de la etapa y el número de etapa.
        // Ejecuta la actualización de forma asíncrona y verifica si la orden existía para actualizarla.

        public async Task ActualizarOF(string OF, string nombreEtapa, string numeroEtapa)
        {
            // Consulta SQL para actualizar los campos nombreEtapa y numeroEtapa de la orden
            string query = @"UPDATE OFs
                            SET nombreEtapa = @nombreEtapa,
                                numeroEtapa = @numeroEtapa
                            WHERE ordenFabricacion = @OF";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Agregamos los parámetros para evitar inyección SQL
                    command.Parameters.AddWithValue("@nombreEtapa", nombreEtapa);
                    command.Parameters.AddWithValue("@numeroEtapa", numeroEtapa);
                    command.Parameters.AddWithValue("@OF", OF);

                    // Ejecutamos la consulta y obtenemos cuántas filas fueron afectadas
                    int filasAfectadas = await command.ExecuteNonQueryAsync();

                    if (filasAfectadas == 0)
                    {
                        // Si no se actualizó ninguna fila, la orden no existe o el parámetro es incorrecto
                        Console.WriteLine("No se actualizó ninguna fila. Verifica que la OF exista.");
                    }
                    else
                    {
                        // Actualización exitosa (comentado para no saturar la consola)
                        // Console.WriteLine("Actualización de etapa exitosa.");
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Cambia el estado de una orden de fabricación(OF) a un nuevo estado y registra la fecha de finalización.
        // Recibe como parámetros el número de orden y el nuevo estado a asignar.
        // Ejecuta la actualización de forma asíncrona y verifica si la orden existía para actualizarla.

        public async Task FinalizarOF(string OF, string estado)
        {
            // Consulta SQL para actualizar el estado y la fecha de finalización de la orden
            string query = @"UPDATE OFs
                            SET status = @estado,
                            fechaFin = GETDATE()
                            WHERE ordenFabricacion = @OF";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Añadimos parámetros para evitar inyección SQL
                    command.Parameters.AddWithValue("@OF", OF);
                    command.Parameters.AddWithValue("@estado", estado);

                    // Ejecutamos la consulta y obtenemos el número de filas afectadas
                    int filasAfectadas = await command.ExecuteNonQueryAsync();

                    if (filasAfectadas == 0)
                    {
                        // Si no se actualizó ninguna fila, puede que la orden no exista
                        Console.WriteLine("No se actualizó el estado. Verifica que la OF exista.");
                    }
                    else
                    {
                        // Actualización exitosa (comentado para no saturar la consola)
                        // Console.WriteLine("Actualización de status de OF exitosa.");
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Inserta un nuevo material en la tabla Materias con los datos proporcionados.
        // Recibe como parámetros un ID, nombre, operación y puesto de trabajo del material.
        // Realiza la inserción de forma asíncrona y verifica si se insertó correctamente.

        public async Task InsertarMaterial(int i_count, string Nombre, string Operacion, string PuestoTrabajo)
        {
            // Consulta SQL para insertar un nuevo registro en la tabla Materias
            string query_InsertMat = @"INSERT INTO Materias (ID, Nombre, Operacion, PuestoTrabajo)
                               VALUES (@ID, @Nombre, @Operacion, @PuestoTrabajo)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query_InsertMat, connection))
                {
                    // Agregamos parámetros para evitar inyección SQL
                    command.Parameters.AddWithValue("@ID", i_count);
                    command.Parameters.AddWithValue("@Nombre", Nombre);
                    command.Parameters.AddWithValue("@Operacion", Operacion);
                    command.Parameters.AddWithValue("@PuestoTrabajo", PuestoTrabajo);

                    // Ejecutamos la inserción y verificamos cuántas filas fueron afectadas
                    int filasAfectadas = await command.ExecuteNonQueryAsync();

                    if (filasAfectadas == 0)
                    {
                        // No se insertó ningún registro (posible error)
                        Console.WriteLine("No se insertó ningún material.");
                    }
                    else
                    {
                        // Inserción exitosa (comentado para no saturar la consola)
                        // Console.WriteLine("Inserción de material exitosa.");
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Verifica si un material con un nombre específico existe en la tabla Materias.
        // Ejecuta una consulta que cuenta cuántos registros coinciden con el nombre dado.
        // Retorna true si el material existe, o false si no se encuentra.

        public async Task<bool> ExisteMaterial(string material)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrimos conexión a la base de datos de forma asíncrona
                await connection.OpenAsync();

                // Consulta SQL para contar registros con el nombre dado
                string query = "SELECT COUNT(*) FROM Materias WHERE Nombre = @material";

                // Añadimos el parámetro para evitar inyección SQL
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@material", material);

                // Ejecutamos la consulta y obtenemos el resultado (número de registros)
                int count = (int)await command.ExecuteScalarAsync();

                // Retornamos true si hay al menos un registro, false en caso contrario
                return count > 0;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Elimina todos los registros de la tabla Materias en la base de datos.
        // Usa una consulta DELETE para borrar todos los materiales almacenados.
        // Se ejecuta de forma asíncrona para no bloquear el hilo principal.

        public async Task EliminarTodosLosMateriales()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrimos la conexión a la base de datos de forma asíncrona
                await connection.OpenAsync();

                // Consulta SQL para eliminar todos los registros de la tabla Materias
                string query = "DELETE FROM Materias"; // Alternativamente, TRUNCATE TABLE si no hay FK
                
                SqlCommand cmd = new SqlCommand(query, connection);
                
                // Ejecutamos la consulta para borrar todos los datos
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Busca y retorna el valor asociado a la cantidad de una Materia Prima específica en una receta y etapa.
        // Primero verifica si existe la Materia Prima con el valor dado y luego obtiene el valor de la cantidad siguiente.
        // Retorna un decimal con el valor si lo encuentra, o null si no existe o no se encuentra el valor.

        public async Task<decimal?> ExtraerValorMMPP(int? ID_Receta, int N_Etapa, decimal MMPP)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Consulta para verificar si existe la Materia Prima con los parámetros indicados
                string query_found = @"SELECT 1
                               FROM ProcesoPrincipal
                               WHERE ID_Receta = @ID_Receta
                                 AND N_Etapa = @N_Etapa
                                 AND Consigna = 'MateriaPrima'
                                 AND Valor = @MMPP;";

                using (SqlCommand checkCommand = new SqlCommand(query_found, connection))
                {
                    // Añadimos los parámetros a la consulta para evitar inyección SQL
                    checkCommand.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                    checkCommand.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                    checkCommand.Parameters.AddWithValue("@MMPP", MMPP);

                    var exists = await checkCommand.ExecuteScalarAsync();

                    if (exists != null)
                    {
                        // Si la Materia Prima existe, obtenemos el valor de la fila siguiente con la cantidad
                        string query = @"SELECT siguiente.Valor
                                 FROM ProcesoPrincipal actual
                                 JOIN ProcesoPrincipal siguiente ON siguiente.ID = actual.ID + 1
                                 WHERE actual.Consigna = 'MateriaPrima'
                                   AND actual.Valor = @MMPP
                                   AND siguiente.Consigna = 'Cantidad'
                                   AND actual.ID_Receta = @ID_Receta
                                   AND actual.N_Etapa = @N_Etapa;";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                            command.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                            command.Parameters.AddWithValue("@MMPP", MMPP);

                            var result = await command.ExecuteScalarAsync();

                            if (result != null && decimal.TryParse(result.ToString(), out decimal valorCantidad))
                            {
                                // Retornamos el valor decimal de la cantidad encontrada
                                return valorCantidad;
                            }
                        }
                    }
                    else
                    {
                        // No se encontró la Materia Prima para los parámetros dados
                    }
                    // Si no se encuentra o no hay valor válido, retornamos null
                    return null;
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Busca y retorna el valor del tiempo asociado a una receta y etapa específica.
        // Verifica si existe una fila con "Consigna" igual a "Tiempo" y luego extrae su valor.
        // Retorna un decimal con el valor del tiempo si lo encuentra, o null si no existe.

        public async Task<decimal?> ExtraerValorTIEMPO(int? ID_Receta, int N_Etapa)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Consulta para verificar si existe un registro con Consigna = 'Tiempo'
                string query_found = @"SELECT 1
                               FROM ProcesoPrincipal
                               WHERE ID_Receta = @ID_Receta
                                 AND N_Etapa = @N_Etapa
                                 AND Consigna = 'Tiempo';
                                 ";

                using (SqlCommand checkCommand = new SqlCommand(query_found, connection))
                {
                    checkCommand.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                    checkCommand.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                    
                    var exists = await checkCommand.ExecuteScalarAsync();

                    if (exists != null)
                    {
                        // Si existe, se obtiene el valor asociado a la consigna 'Tiempo'
                        string query = @"SELECT valor
                                 FROM ProcesoPrincipal 
                                   WHERE Consigna = 'Tiempo'
                                   AND ID_Receta = @ID_Receta
                                   AND N_Etapa = @N_Etapa;";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                            command.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                            
                            var result = await command.ExecuteScalarAsync();

                            if (result != null && decimal.TryParse(result.ToString(), out decimal valorCantidad))
                            {
                                // Retorna el valor decimal encontrado
                                return valorCantidad;
                            }
                        }
                    }
                    else
                    {
                        // No se encontró ningún registro con Consigna = 'Tiempo'
                    }
                    // Retorna null si no hay datos válidos
                    return null;
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Busca y devuelve un valor decimal asociado al 'Operador' para una receta y etapa específica.
        // Verifica si existe algún registro con Tipo = 'Operador' en la tabla ProcesoPrincipal.
        // Retorna el valor decimal si se encuentra, o null si no existe.

        public async Task<decimal?> ExtraerOperario(int? ID_Receta, int N_Etapa)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Consulta para verificar si hay un registro con Tipo 'Operador' para la receta y etapa dadas
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
                        // Retorna el valor decimal encontrado
                        return valor;
                    }
                    else
                    {
                        // No se encontró el valor del operador, retorna null
                        return null;
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Busca y devuelve el valor decimal de la agitacion para una receta, etapa y consigna específicas.
        // Consulta la tabla ProcesoAgitacion filtrando por ID_Receta, N_Etapa, Tipo 'Agitacion' y la consigna dada.
        // Retorna el valor si lo encuentra, o null si no hay datos.

        public async Task<decimal?> ExtraerAgitacion(int? ID_Receta, int N_Etapa,string Consigna)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Consulta para obtener el valor de agitacion según los parámetros              
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
                    // Agregar los parámetros para evitar inyección SQL
                    command.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                    command.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                    command.Parameters.AddWithValue("@Consigna", Consigna);

                    var result = await command.ExecuteScalarAsync();

                    if (result != null && decimal.TryParse(result.ToString(), out decimal valor))
                    {
                        // Retorna el valor decimal encontrado
                        return valor;
                    }
                    else
                    {
                        // No se encontró el valor, retorna null
                        return null;
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Busca y devuelve el valor decimal de temperatura para una receta, etapa y consigna específicas.
        // Consulta la tabla ProcesoTemperatura filtrando por ID_Receta, N_Etapa, Tipo 'Temperatura' y la consigna dada.
        // Retorna el valor si lo encuentra, o null si no hay datos disponibles.

        public async Task<decimal?> ExtraerTemperatura(int? ID_Receta, int N_Etapa, string Consigna)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Consulta SQL para obtener el valor de temperatura según los parámetros recibidos
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
                    // Agregamos parámetros para evitar inyección SQL
                    command.Parameters.AddWithValue("@ID_Receta", ID_Receta);
                    command.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                    command.Parameters.AddWithValue("@Consigna", Consigna);

                    // Ejecutamos la consulta y obtenemos el primer valor de la primera fila
                    var result = await command.ExecuteScalarAsync();

                    // Verificamos que el resultado no sea nulo y sea un decimal válido
                    if (result != null && decimal.TryParse(result.ToString(), out decimal valor))
                    {
                        // Retornamos el valor encontrado
                        return valor;
                    }
                    else
                    {
                        // No se encontró ningún valor, retornamos null
                        return null;
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Busca y devuelve el ID de una receta dado su nombre.
        // Ejecuta una consulta en la tabla Recetas para obtener el ID correspondiente.
        // Retorna el ID si lo encuentra, o null si no existe la receta.

        public async Task<int?> ObtenerIDReceta(string NombreReceta)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Consulta para obtener el ID de la receta según el nombre
                string query = @"
                        SELECT ID
                        FROM Recetas
                        WHERE NombreReceta = @NombreReceta;
                        ";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Se agrega el parámetro para prevenir inyección SQL
                    command.Parameters.AddWithValue("@NombreReceta", NombreReceta);

                    // Ejecutamos la consulta y obtenemos un solo valor (ID)
                    var result = await command.ExecuteScalarAsync();

                    // Verificamos que el resultado no sea nulo y sea un entero válido
                    if (result != null && int.TryParse(result.ToString(), out int id))
                    {
                        // Retornamos el ID encontrado
                        return id;
                    }
                    else
                    {
                        // No se encontró la receta, retornamos null
                        return null;
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Obtiene una lista de números de Orden de Fabricación(OF) que están en estado 'Lanzada'.
        // Ejecuta una consulta para seleccionar todos los números únicos de OF con ese estado.
        // Devuelve la lista de OFs como una lista de strings.

        public async Task<List<string>> ObtenerOFLanzadas()
        {
            var listaOFs = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Consulta para obtener los números de OrdenFabricacion con estado 'Lanzada'
                string query = @"
                        SELECT DISTINCT OrdenFabricacion
                        FROM OFs
                        WHERE Status = 'Lanzada'";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    // Leemos cada fila y extraemos la OrdenFabricacion
                    while (await reader.ReadAsync())
                    {
                        string orden = reader[0]?.ToString();
                        if (!string.IsNullOrWhiteSpace(orden))
                        {
                            // Agregamos a la lista si no está vacía
                            listaOFs.Add(orden);
                        }
                    }
                }
            }

            return listaOFs;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Ejecuta una consulta SQL para obtener materias primas y sus cantidades.
        // Lee cada fila del resultado y guarda los valores en un diccionario.
        // Devuelve una lista con esos diccionarios, cada uno representando una materia prima.

        public async Task<List<Dictionary<string, object>>> ObtenerMateriasPrimas(string query)
        {
            var listaMaterias = new List<Dictionary<string, object>>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    // Lee cada fila y crea un diccionario con materiaPrima y cantidad
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

        // ---------------------------------------------------------------------------------------------------------------------------

        // Consulta los datos reales de materias primas para una orden de fabricación específica.
        // Lee los valores de varios componentes químicos de la base de datos.
        // Devuelve un diccionario con el nombre corto y el valor de cada materia prima.

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
                    // Agrega el parámetro para evitar inyección SQL
                    command.Parameters.AddWithValue("@ordenFabricacion", ordenFabricacion);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Mapea cada columna a una clave del diccionario con nombre corto
                            // En caso de que se amplie o se Modifique alguna MMPP cambiarla aqui
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

        // ---------------------------------------------------------------------------------------------------------------------------

        // Obtiene el nombre de una etapa en una receta específica.
        // Busca en la tabla Etapas usando el ID de receta y el número de etapa.
        // Devuelve el nombre como string o null si no se encuentra.

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
                    // Parámetros para evitar inyección SQL
                    command.Parameters.AddWithValue("@ID_Etapa", ID_Etapa);
                    command.Parameters.AddWithValue("@ID_Receta", ID_Receta);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Si encuentra el registro, extrae el nombre
                            Nombre_Etapa = reader["Nombre"]?.ToString();
                        }
                    }
                }
            }

            return Nombre_Etapa;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Método asincrónico que obtiene los datos finales de materias primas (MMPP) para una orden de fabricación específica.
        // Consulta la tabla `MMPP_Finales` y devuelve un objeto `MMPPFinal` con todos los valores de control teórico y real.
        // Si no se encuentra ningún registro, retorna `null`.
        
        public async Task<Models.MMPPFinal> ObtenerMMPPFinales(string OF)
        {
            // Inicializamos el resultado como null para el caso que no se encuentre registro
            Models.MMPPFinal resultado = null;

            // Crear y abrir conexión con la base de datos usando el connectionString definido
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrimos la conexión de forma asíncrona
                await connection.OpenAsync();

                // Consulta SQL para obtener la primera fila que coincida con la orden de fabricación (OF)
                string query = @"
                            SELECT TOP 1 *
                            FROM MMPP_Finales
                            WHERE OrdenFabricacion = @OF";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Agregamos el parámetro para evitar inyección SQL y pasar la OF
                    command.Parameters.AddWithValue("@OF", OF);

                    // Ejecutamos el comando y obtenemos el lector de datos
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // Si hay una fila disponible la leemos
                        if (await reader.ReadAsync())
                        {
                            // Mapeamos los campos del lector a la instancia de Models.MMPPFinal
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
            // Retornamos el objeto con los datos obtenidos o null si no se encontró
            return resultado;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Método asincrónico que obtiene todas las órdenes de fabricación finalizadas.
        // Consulta la tabla `MMPP_Finales`, extrae los valores de `OrdenFabricacion` y los devuelve en formato JSON.

        public async Task<string> ExtraerOFFinalizadas()
        {
            // Lista que almacenará las órdenes extraídas de la base de datos
            List<string> ordenes = new List<string>();

            // Establecemos una conexión con la base de datos utilizando el connection string
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrimos la conexión de forma asíncrona
                await connection.OpenAsync();

                // Consulta SQL para obtener todas las órdenes de fabricación registradas en MMPP_Finales
                string query = @"SELECT OrdenFabricacion FROM MMPP_Finales;";

                // Ejecutamos la consulta
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    // Recorremos cada fila del resultado
                    while (await reader.ReadAsync())
                    {
                        // Obtenemos el valor de la columna OrdenFabricacion y lo agregamos a la lista
                        string of = reader["OrdenFabricacion"].ToString();
                        ordenes.Add(of);
                    }
                }
            }
            // Convertimos la lista de órdenes a formato JSON y la retornamos
            // Requiere: using Newtonsoft.Json;
            return JsonConvert.SerializeObject(ordenes);
        }

        // ---------------------------------------------------------------------------------------------------------------------------

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