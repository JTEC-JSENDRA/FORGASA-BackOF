using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using API_SAP.Clases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_SAP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanzadasController : ControllerBase
    {
        // ---------------------------------------------------------------------------------------------------------------------------

        // Método privado que configura y devuelve una conexión a la base de datos SQL Server

        private SQLServerManager BBDD_Config()
        {
            string nombreServidor = Environment.MachineName; // Nombre del PC donde corre el servidor
            string ServidorSQL = $"{nombreServidor}\\SQLEXPRESS"; // Nombre completo del servidor SQL
            string BaseDatos = "Recetas"; // Nombre de la base de datos
            string Usuario = "sa"; // Usuario SQL
            string Password = "GomezMadrid2021"; // Contraseña
            // Cadena de conexión completa
            string connectionString = $"Data Source={ServidorSQL};Initial Catalog={BaseDatos};User ID={Usuario};Password={Password};";

            return new SQLServerManager(connectionString);
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // GET: api/Lanzadas
        // Devuelve un listado JSON con todas las órdenes lanzadas

        [HttpGet] 
        public async Task<string> Get()
        {
            // 1️. Crea una conexión con la base de datos SQL Server
            SQLServerManager BBDD = BBDD_Config();

            // 2️. Llama a un método que consulta la base de datos y devuelve una lista de objetos tipo JObject
            //     Probablemente está consultando todas las órdenes "lanzadas"
            List<JObject> jsonObjectList = await BBDD.ListadoLanzadas();

            // 3️. Convierte la lista a formato JSON con sangría para que sea más fácil de leer
            string jsonResult = JsonConvert.SerializeObject(jsonObjectList, Formatting.Indented);

            // 4️. Devuelve el JSON como respuesta de la API
            return jsonResult;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // GET: api/Lanzadas/MateriasPorOrden
        // Este endpoint devuelve un JSON con las materias primas que están planificadas por cada orden de fabricación lanzada

        [HttpGet("MateriasPorOrden")]
        public async Task<string> ObtenerMateriasPorOrden()
        {
            // Conectamos a la base de datos usando un método ya creado
            SQLServerManager BBDD = BBDD_Config();

            // Obtenemos una lista de todas las órdenes de fabricación que han sido lanzadas
            var ordenes = await BBDD.ObtenerOFLanzadas();

            // Creamos una lista vacía donde guardaremos los datos finales que enviaremos como respuesta
            var resultadoFinal = new List<Dictionary<string, string>>();

            // Diccionario que traduce los nombres originales de las materias primas a nombres más simples para el JSON
            // ->> EN CASO DE QUE SE AMPLIEN LAS MATERIAS PRIMAS HAY QUE AÑADIRALAS AQUI CON EL MISMO NOMBRE QUE SALE DESDE SAP
            var mapaMateria = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"LC70-01", "lc70"},
                {"LC80-01", "lc80"},
                {"HL26(10-16)(0-0-8)-01", "hl26"},
                {"AGUA", "agua"},
                {"AGUA RECUPERADA", "aguaRecuperada"},
                {"HL PRUEBAS", "antiespumante"},
                {"CALCIO LIGNOSULFONATO SOLIDO", "ligno"},
                {"POTASA LIQUIDA 50%", "potasa"},
                {"POTASA LIQUIDA 47%", "potasa"} // Ambas potasas se agrupan bajo el mismo nombre
            };

            // Recorremos una a una las órdenes lanzadas
            foreach (var orden in ordenes)
            {
                // Convertimos la orden en texto (por si viene como número u otro tipo de dato)
                string ordenFabricacion = orden?.ToString() ?? string.Empty;

                // Hacemos una consulta a la base de datos para obtener las materias primas y cantidades asociadas a esa orden
                var materias = await BBDD.ObtenerMateriasPrimas($@"
                    SELECT materiaPrima, cantidad 
                    FROM Materiales
                    WHERE ordenFabricacion = '{ordenFabricacion}'");

                // Creamos un objeto tipo diccionario para guardar los datos de esta orden
                var jsonObj = new Dictionary<string, string> { { "ordenFabricacion", ordenFabricacion } };

                // Inicializamos todas las materias con el valor "0" (por si alguna no se usa en esa orden)
                foreach (var prop in mapaMateria.Values)
                {
                    jsonObj[prop] = "0";
                }

                // Recorremos los resultados de la base de datos para asignar los valores reales
                foreach (var mat in materias)
                {
                    // Extraemos los datos de materia prima y cantidad
                    if (mat.TryGetValue("materiaPrima", out var matNameObj) && mat.TryGetValue("cantidad", out var cantidadObj))
                    {
                        string matName = matNameObj?.ToString() ?? "";
                        string cantidad = cantidadObj?.ToString() ?? "0";

                        // Si la materia está en el diccionario de nombres conocidos, la añadimos con su cantidad real
                        if (mapaMateria.TryGetValue(matName, out string propJson))
                        {
                            jsonObj[propJson] = cantidad;
                        }
                    }
                }
                // Agregamos este resultado (orden y sus materias) a la lista final
                resultadoFinal.Add(jsonObj);
            }

            // Convertimos toda la lista de resultados a un JSON con formato bonito (indentado) y lo devolvemos como respuesta
            return JsonConvert.SerializeObject(resultadoFinal, Formatting.Indented);
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // GET: api/Lanzadas/MateriasPorOrdenReales
        // Esta es una función que responde a una petición GET en la ruta "MateriasPorOrdenReales"
        // y devuelve un JSON con las materias primas reales usadas en cada orden lanzada.

        [HttpGet("MateriasPorOrdenReales")]
        public async Task<string> ObtenerMateriasPorOrdenReales()
        {
            // Creamos una instancia para manejar la base de datos, usando una función de configuración.
            SQLServerManager BBDD = BBDD_Config();

            // Obtenemos todas las órdenes lanzadas de la base de datos de forma asíncrona.
            var ordenes = await BBDD.ObtenerOFLanzadas();

            // Lista que guardará el resultado final: cada elemento será un diccionario con datos de una orden.
            var resultadoFinal = new List<Dictionary<string, string>>();

            // Diccionario que relaciona nombres de materias primas tal como están en SAP (la fuente) -> SI SE AMPLIAN HAY QUE AÑADIRLAS
            // con los nombres que vamos a usar en el JSON que retornaremos.
            // Se usa StringComparer.OrdinalIgnoreCase para que no importe mayúsculas/minúsculas.
            var mapaMateria = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"LC70-01", "lc70"},
                {"LC80-01", "lc80"},
                {"HL26(10-16)(0-0-8)-01", "hl26"},
                {"AGUA", "agua"},
                {"AGUA RECUPERADA", "aguaRecuperada"},
                {"HL PRUEBAS", "antiespumante"},
                {"CALCIO LIGNOSULFONATO SOLIDO", "ligno"},
                {"POTASA LIQUIDA 50%", "potasa"},
                {"POTASA LIQUIDA 47%", "potasa"}
            };

            // Para cada orden que obtuvimos
            foreach (var orden in ordenes)
            {
                // Convertimos la orden a string (por si es nulo, le ponemos cadena vacía)
                string ordenFabricacion = orden?.ToString() ?? string.Empty;

                // Obtenemos las materias primas reales usadas para esa orden de forma asíncrona.
                var materias = await BBDD.ObtenerMateriasPrimasReales(ordenFabricacion);

                // Creamos un diccionario que representará un objeto JSON para esta orden,
                // con los nombres ya mapeados y los valores que corresponden.
                // Si no hay un valor para alguna materia, ponemos "0" por defecto.
                var jsonObj = new Dictionary<string, string>
                {
                    { "ordenFabricacion", ordenFabricacion },
                    { "lc70", materias.GetValueOrDefault("lc70", "0") },
                    { "lc80", materias.GetValueOrDefault("lc80", "0") },
                    { "hl26", materias.GetValueOrDefault("hl26", "0") },
                    { "agua", materias.GetValueOrDefault("agua", "0") },
                    { "aguaRecuperada", materias.GetValueOrDefault("aguaRecuperada", "0") },
                    { "antiespumante", materias.GetValueOrDefault("antiespumante", "0") },
                    { "ligno", materias.GetValueOrDefault("ligno", "0") },
                    { "potasa", materias.GetValueOrDefault("potasa", "0") }
                };

                // Agregamos este objeto JSON a la lista de resultados.
                resultadoFinal.Add(jsonObj);
            }

            // Convertimos la lista de diccionarios a formato JSON con una indentación para que sea legible,
            // y lo devolvemos como resultado de la función.
            return JsonConvert.SerializeObject(resultadoFinal, Formatting.Indented);
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // GET: api/Lanzadas/OF/{OF}
        // Esta función responde a una petición GET en la ruta "OF/{OF}",
        // donde {OF} es un parámetro que representa una orden de fabricación específica.
        // Devuelve un JSON con las materias primas finalizadas para esa orden.

        [HttpGet("OF/{OF}")]
        public async Task<string> ObtenerMMPPFinalizadasporOF(string OF)
        {
            // Creamos una instancia para manejar la base de datos, usando una función de configuración.
            SQLServerManager BBDD = BBDD_Config();

            // Llamamos a la base de datos para obtener las materias primas finalizadas
            // para la orden de fabricación que nos pasan como parámetro (OF).
            // Esto se hace de forma asíncrona porque puede tardar un poco.
            var MMPPFinales_OF = await BBDD.ObtenerMMPPFinales(OF);

            // Convertimos el resultado a formato JSON con indentación para que sea legible
            // y lo devolvemos como respuesta de la función.
            return JsonConvert.SerializeObject(MMPPFinales_OF, Formatting.Indented);
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // GET: api/Lanzadas/OF_Finalizadas
        // Esta función responde a una petición GET en la ruta "OF_Finalizadas"
        // y devuelve una lista de órdenes de fabricación que están finalizadas.

        [HttpGet("OF_Finalizadas")]
        public async Task<IActionResult> ObtenerOF_Finalizadas()
        {
            // Creamos una instancia para manejar la base de datos, usando una función de configuración.
            SQLServerManager BBDD = BBDD_Config();

            // Llamamos a la base de datos para obtener las órdenes finalizadas en formato JSON (string).
            var OF_FinalizadasJson = await BBDD.ExtraerOFFinalizadas();

            // Convertimos (deserializamos) ese JSON recibido en una lista de cadenas (strings),
            // es decir, obtenemos una lista con los nombres o identificadores de las órdenes finalizadas.
            var listaOF = JsonConvert.DeserializeObject<List<string>>(OF_FinalizadasJson);

            // Devolvemos la lista usando Ok(), que en ASP.NET Core la convierte automáticamente a JSON
            // y responde con un código HTTP 200 (OK).
            return Ok(listaOF); // ASP.NET Core lo convierte a JSON automáticamente
        }

        // ---------------------------------------------------------------------------------------------------------------------------

    }
}