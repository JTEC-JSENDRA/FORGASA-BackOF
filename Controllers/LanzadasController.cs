using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using API_SAP.Clases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/*
namespace API_SAP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanzadasController : ControllerBase
    {
        private SQLServerManager BBDD_Config()
        {
            string nombreServidor = Environment.MachineName;
            string ServidorSQL = $"{nombreServidor}\\SQLEXPRESS";
            string BaseDatos = "Recetas";
            string Usuario = "sa";
            string Password = "GomezMadrid2021";
            string connectionString = $"Data Source={ServidorSQL};Initial Catalog={BaseDatos};User ID={Usuario};Password={Password};";

            return new SQLServerManager(connectionString);
        }

        // GET: api/Lanzadas
        [HttpGet]
        public async Task<string> Get()
        {
            SQLServerManager BBDD = BBDD_Config();

            List<JObject> jsonObjectList = await BBDD.ListadoLanzadas();

            string jsonResult = JsonConvert.SerializeObject(jsonObjectList, Formatting.Indented);

            return jsonResult;
        }

        // GET: api/Lanzadas/MateriasPorOrden
        [HttpGet("MateriasPorOrden")]
        public async Task<string> ObtenerMateriasPorOrden()
        {
            SQLServerManager BBDD = BBDD_Config();

            // 1. Obtener todas las órdenes lanzadas
            var ordenes = await BBDD.ObtenerOFLanzadas();

            //Console.WriteLine($"OFLanzadas: {string.Join(", ", ordenes)}");

            var resultadoFinal = new List<Dictionary<string, string>>();

            // Diccionario para mapear nombres en DB a nombres de propiedad JSON
            var mapaMateria = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {"LC70-01", "lc70"},
                    {"LC80-01", "lc80"},
                    {"HL26", "hl26"},
                    {"AGUA", "agua"},
                    {"AGUA RECUPERADA", "aguaRecuperada"},
                    {"HL PRUEBAS", "antiespumante"},
                    {"CALCIO LIGNOSULFONATO SOLIDO", "ligno"},
                    {"POTASA LIQUIDA 50%", "potasa"},
                    {"POTASA LIQUIDA 47%", "potasa"}
                };

            foreach (var orden in ordenes)
            {
                string ordenFabricacion = orden?.ToString() ?? string.Empty;

                // 2. Obtener materias primas para esta orden
                var materias = await BBDD.ObtenerMateriasPrimas($@"
                        SELECT materiaPrima, cantidad 
                        FROM Materiales
                        WHERE ordenFabricacion = '{ordenFabricacion}'
                    ");

                //Console.WriteLine($"Lista de materias primas para orden {ordenFabricacion}: {JsonConvert.SerializeObject(materias)}");

                // 3. Crear objeto JSON para esta orden con valores inicializados a "0"
                var jsonObj = new Dictionary<string, string>
                    {
                        { "ordenFabricacion", ordenFabricacion }
                    };

                // Inicializa todas las materias en 0
                foreach (var prop in mapaMateria.Values)
                {
                    jsonObj[prop] = "0";
                }

                // 4. Asignar cantidades encontradas
                foreach (var mat in materias)
                {
                    if (mat.TryGetValue("materiaPrima", out var matNameObj) && mat.TryGetValue("cantidad", out var cantidadObj))
                    {
                        string matName = matNameObj?.ToString() ?? "";
                        string cantidad = cantidadObj?.ToString() ?? "0";

                        if (mapaMateria.TryGetValue(matName, out string propJson))
                        {
                            jsonObj[propJson] = cantidad;
                        }
                    }
                }

                resultadoFinal.Add(jsonObj);
            }

            Console.WriteLine($"[DEBUG MMPP] Versión Final: {JsonConvert.SerializeObject(resultadoFinal, Formatting.Indented)}");
            return JsonConvert.SerializeObject(resultadoFinal, Formatting.Indented);
        }

        [HttpGet("MateriasPorOrdenReales")]
        public async Task<string> ObtenerMateriasPorOrdenReales()
        {
            SQLServerManager BBDD = BBDD_Config();

            // 1. Obtener todas las órdenes lanzadas
            var ordenes = await BBDD.ObtenerOFLanzadas();

            var resultadoFinal = new List<Dictionary<string, string>>();

            var mapaMateria = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {"LC70-01", "lc70"},
                    {"LC80-01", "lc80"},
                    {"HL26", "hl26"},
                    {"AGUA", "agua"},
                    {"AGUA RECUPERADA", "aguaRecuperada"},
                    {"HL PRUEBAS", "antiespumante"},
                    {"CALCIO LIGNOSULFONATO SOLIDO", "ligno"},
                    {"POTASA LIQUIDA 50%", "potasa"},
                    {"POTASA LIQUIDA 47%", "potasa"}
                };

            foreach (var orden in ordenes)
            {
                string ordenFabricacion = orden?.ToString() ?? string.Empty;

                // 2. Obtener materias primas para esta orden
                var materias = await BBDD.ObtenerMateriasPrimasReales(ordenFabricacion);

                // 3. Crear objeto JSON para esta orden con valores inicializados a "0"
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

                resultadoFinal.Add(jsonObj);
            }

            var jsonFinal = JsonConvert.SerializeObject(resultadoFinal, Formatting.Indented);
            Console.WriteLine("JSON Final completo:\n" + jsonFinal);

            return jsonFinal;
        }

        [HttpGet("OF/{OF}")]
        public async Task<string> ObtenerMMPPFinalizadasporOF(string OF)
        {
            SQLServerManager BBDD = BBDD_Config();

            // 1. Obtener todas las órdenes lanzadas
            var MMPPFinales_OF = await BBDD.ObtenerMMPPFinales(OF);

            // Aquí el console log:
            Console.WriteLine("Datos obtenidos de MMPPFinales_OF:");
            if (MMPPFinales_OF != null)
            {
                Console.WriteLine("[DEBUG EMERGENTE]");
                Console.WriteLine(JsonConvert.SerializeObject(MMPPFinales_OF, Formatting.Indented));
                Console.WriteLine("[---------------]");
            }
            else
            {
                Console.WriteLine("No se encontraron datos para la OF: " + OF);
            }

            return JsonConvert.SerializeObject(MMPPFinales_OF, Formatting.Indented);
        }

    }
}
*/
namespace API_SAP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanzadasController : ControllerBase
    {
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

        // GET: api/Lanzadas
        // Devuelve un listado JSON con todas las órdenes lanzadas
        [HttpGet]
        public async Task<string> Get()
        {
            SQLServerManager BBDD = BBDD_Config();
            List<JObject> jsonObjectList = await BBDD.ListadoLanzadas();

            // Convierte la lista a JSON con formato bonito
            string jsonResult = JsonConvert.SerializeObject(jsonObjectList, Formatting.Indented);
            return jsonResult;
        }

        // GET: api/Lanzadas/MateriasPorOrden
        // Devuelve un JSON con las materias primas planificadas por cada orden lanzada
        [HttpGet("MateriasPorOrden")]
        public async Task<string> ObtenerMateriasPorOrden()
        {
            SQLServerManager BBDD = BBDD_Config();

            var ordenes = await BBDD.ObtenerOFLanzadas();
            var resultadoFinal = new List<Dictionary<string, string>>();

            // Diccionario que traduce nombres de materias a nombres JSON amigables
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
                {"POTASA LIQUIDA 47%", "potasa"} // Ambas se resumen como "potasa"
            };

            foreach (var orden in ordenes)
            {
                string ordenFabricacion = orden?.ToString() ?? string.Empty;

                // Consulta a base de datos para materias primas planificadas por orden
                var materias = await BBDD.ObtenerMateriasPrimas($@"
                    SELECT materiaPrima, cantidad 
                    FROM Materiales
                    WHERE ordenFabricacion = '{ordenFabricacion}'");

                var jsonObj = new Dictionary<string, string> { { "ordenFabricacion", ordenFabricacion } };

                // Inicializa todas las materias conocidas en 0
                foreach (var prop in mapaMateria.Values)
                {
                    jsonObj[prop] = "0";
                }

                // Asigna los valores reales encontrados en base de datos
                foreach (var mat in materias)
                {
                    if (mat.TryGetValue("materiaPrima", out var matNameObj) && mat.TryGetValue("cantidad", out var cantidadObj))
                    {
                        string matName = matNameObj?.ToString() ?? "";
                        string cantidad = cantidadObj?.ToString() ?? "0";

                        if (mapaMateria.TryGetValue(matName, out string propJson))
                        {
                            jsonObj[propJson] = cantidad;
                        }
                    }
                }

                resultadoFinal.Add(jsonObj);
            }

            // Devuelve el JSON completo
            return JsonConvert.SerializeObject(resultadoFinal, Formatting.Indented);
        }

        // GET: api/Lanzadas/MateriasPorOrdenReales
        // Devuelve un JSON con materias primas reales (usadas) por orden lanzada
        [HttpGet("MateriasPorOrdenReales")]
        public async Task<string> ObtenerMateriasPorOrdenReales()
        {
            SQLServerManager BBDD = BBDD_Config();
            var ordenes = await BBDD.ObtenerOFLanzadas();
            var resultadoFinal = new List<Dictionary<string, string>>();
            // ASEGURANOS DE METER EL MISMO NOMBRE UQE LEEMOS DE SAP
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

            foreach (var orden in ordenes)
            {
                string ordenFabricacion = orden?.ToString() ?? string.Empty;
                var materias = await BBDD.ObtenerMateriasPrimasReales(ordenFabricacion);

                // Crea el JSON por orden, usando GetValueOrDefault para valores faltantes
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

                resultadoFinal.Add(jsonObj);
            }

            return JsonConvert.SerializeObject(resultadoFinal, Formatting.Indented);
        }

        // GET: api/Lanzadas/OF/{OF}
        // Devuelve materias primas finalizadas para una orden de fabricación específica
        [HttpGet("OF/{OF}")]
        public async Task<string> ObtenerMMPPFinalizadasporOF(string OF)
        {
            SQLServerManager BBDD = BBDD_Config();

            var MMPPFinales_OF = await BBDD.ObtenerMMPPFinales(OF);

            return JsonConvert.SerializeObject(MMPPFinales_OF, Formatting.Indented);
        }

        // GET: api/Lanzadas/OF_Finalizadas
        // Devuelve materias primas finalizadas para una orden de fabricación específica
        [HttpGet("OF_Finalizadas")]
        public async Task<IActionResult> ObtenerOF_Finalizadas()
        {
            SQLServerManager BBDD = BBDD_Config();

            var OF_FinalizadasJson = await BBDD.ExtraerOFFinalizadas();

            // Deserializar el JSON a un array/lista de strings
            var listaOF = JsonConvert.DeserializeObject<List<string>>(OF_FinalizadasJson);

            return Ok(listaOF); // ASP.NET Core lo convierte a JSON automáticamente
        }
    }
}