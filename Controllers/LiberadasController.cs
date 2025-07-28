using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using API_SAP.Clases;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using System.Xml.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API_SAP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiberadasController : ControllerBase
    {
        // ---------------------------------------------------------------------------------------------------------------------------

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

        // ---------------------------------------------------------------------------------------------------------------------------

        // API ENCARGADA DE DESCARGAR EL ARCHIVO DE SAP -> AHORA MISMO TRABAJANDO DESDE ARCHIVO SAP LOCAL 

        [HttpGet("SAP/{Centro}")]
        public async Task<string> Get_SAP(string Centro = "FO01")
        {

            string urlServicio = "http://SAPPRD.samca.net:8001/sap/bc/srt/rfc/sap/zmes_ws_ofs/010/zmes_ws_ofs/zmes_bn_ofs"; //datos

            string xmlSolicitud = @$"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:sap-com:document:sap:rfc:functions"">
                                    <soapenv:Header/>
                                    <soapenv:Body>
                                        <urn:ZMES_GET_OFS>
                                            <IV_CENTRO>FO01</IV_CENTRO>
                                            <!--Optional:-->
                                            <IV_MATERIAL></IV_MATERIAL>
                                            <!--Optional:-->
                                            <IV_PUESTO></IV_PUESTO>
                                        </urn:ZMES_GET_OFS>
                                    </soapenv:Body>
                                    </soapenv:Envelope>";


            string soapResponse;
            string rutaXML;

            // 


            // ---------------- CUANTO SE HABILITE SAP HAY QUE COMENTAR ESTAS LÍNEAS

            //string soapResponse;
            //string rutaXML = @"C:\Users\ZMES_GET_OFS_RESPONSE.xml";
            //string rutaXML;
            // ---------------- CUANTO SE HABILITE SAP HAY QUE DESCOMENTAR ESTAS LÍNEAS

            // Configurar cliente HttpClient
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("SOAPAction", "urn:sap-com:document:sap:rfc:functions:ZMES_GET_OFS");
                httpClient.DefaultRequestHeaders.Add("Accept", "text/xml");

                //string RutaXML = @"C:\Users\ZMES_GET_OFS_RESPONSE.xml";

                // Configurar el contenido de la solicitud SOAP
                var content = new StringContent(xmlSolicitud, Encoding.UTF8, "text/xml");

                // Enviar solicitud POST al servicio web SOAP
                HttpResponseMessage response = await httpClient.PostAsync(urlServicio, content);

                // Leer la respuesta SOAP
                soapResponse = await response.Content.ReadAsStringAsync();

                // Mostrar la respuesta en la consola
                Console.WriteLine("Respuesta de la API SOAP:");
                Console.WriteLine(soapResponse);

                try
                {

                    rutaXML = @"C:\Users\ZMES_GET_OFS_RESPONSE.xml";

                    //string carpetaDestino = @"C:\Temp";

                    //if (!Directory.Exists(carpetaDestino))
                    //{
                    //    Directory.CreateDirectory(carpetaDestino); // Crea la carpeta si no existe
                    //}

                    //rutaXML = Path.Combine(carpetaDestino, "ZMES_GET_OFS_RESPONSE.xml");

                    // Guarda el contenido XML en el archivo
                    System.IO.File.WriteAllText(rutaXML, soapResponse);

                    Console.WriteLine($"Archivo guardado correctamente en: {rutaXML}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al guardar el archivo: {ex.Message}");
                }
            }
            return soapResponse;
        }

        // ---------------------------------------------------------------------------------------------------------------------------



        [HttpGet("SAP/MMPP/{OF}")]
        public async Task<string> Send_SAP_Confirmation(string OF)
        {
            // 1. Conectar con BBDD y obtener datos reales
            SQLServerManager BBDD = BBDD_Config();
            var MMPPFinales_SAP = await BBDD.ObtenerMMPPFinales(OF);

            // 2. Validar existencia de datos
            if (MMPPFinales_SAP == null)
                return $"❌ No se encontraron datos de MMPP para la orden {OF}";

            // 3. Formatear fecha para SAP (yyyyMMdd)
            string fechaFormateada = MMPPFinales_SAP.FechaInsercion.ToString("yyyyMMdd");

            // 4. Construir XML SOAP
            string xmlSolicitud = @$"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:sap-com:document:sap:rfc:functions"">
                                       <soapenv:Header/>
                                       <soapenv:Body>
                                          <urn:ZMES_IN_NOTIF_CONSUMO>
                                             <I_DATE>{fechaFormateada}</I_DATE>
                                             <I_MESSAGE>00001</I_MESSAGE>
                                             <I_RCVPRN>PROTEO</I_RCVPRN>
                                             <I_WERKS>MY01</I_WERKS>
                                             <I_LINEA>
                                                <item><MATNR>000000000030061298</MATNR><CHARG/><LGORT/><MENGE>{MMPPFinales_SAP.Solidos_1_CR}</MENGE><MEINS>KG</MEINS><AUFNR>{OF}</AUFNR></item>
                                                <item><MATNR>000000000030060566</MATNR><CHARG/><LGORT/><MENGE>{MMPPFinales_SAP.Solidos_2_CR}</MENGE><MEINS>KG</MEINS><AUFNR>{OF}</AUFNR></item>
                                                <item><MATNR>000000000030061298</MATNR><CHARG/><LGORT/><MENGE>{MMPPFinales_SAP.Solidos_3_CR}</MENGE><MEINS>KG</MEINS><AUFNR>{OF}</AUFNR></item>
                                                <item><MATNR>000000000030060753</MATNR><CHARG/><LGORT/><MENGE>{MMPPFinales_SAP.Agua_CR}</MENGE><MEINS>L</MEINS><AUFNR>{OF}</AUFNR></item>
                                                <item><MATNR>000000000030080224</MATNR><CHARG/><LGORT/><MENGE>{MMPPFinales_SAP.Agua_Recu_CR}</MENGE><MEINS>L</MEINS><AUFNR>{OF}</AUFNR></item>
                                                <item><MATNR>HL PRUEBAS</MATNR><CHARG/><LGORT/><MENGE>{MMPPFinales_SAP.Antiespumante_CR}</MENGE><MEINS>KG</MEINS><AUFNR>{OF}</AUFNR></item>
                                                <item><MATNR>000000000030060518</MATNR><CHARG/><LGORT/><MENGE>{MMPPFinales_SAP.Ligno_CR}</MENGE><MEINS>KG</MEINS><AUFNR>{OF}</AUFNR></item>
                                                <item><MATNR>000000000030060498</MATNR><CHARG/><LGORT/><MENGE>{MMPPFinales_SAP.Potasa_CR}</MENGE><MEINS>KG</MEINS><AUFNR>{OF}</AUFNR></item>
                                             </I_LINEA>
                                          </urn:ZMES_IN_NOTIF_CONSUMO>
                                       </soapenv:Body>
                                    </soapenv:Envelope>";

            // 5. Enviar solicitud a SAP
            string soapResponse;
            string urlServicio = "http://prd.samca.net:8001/sap/bc/srt/rfc/sap/zmes_in_notif_consumo/010/zws_mes_in_notif_consumo/zbn_mes_in_notif_consumo";

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("SOAPAction", "urn:sap-com:document:sap:rfc:functions:ZMES_IN_NOTIF_CONSUMO");
                httpClient.DefaultRequestHeaders.Add("Accept", "text/xml");

                var content = new StringContent(xmlSolicitud, Encoding.UTF8, "text/xml");

                HttpResponseMessage response = await httpClient.PostAsync(urlServicio, content);

                if (!response.IsSuccessStatusCode)
                    return $"❌ Error HTTP al llamar SAP: {response.StatusCode}";

                soapResponse = await response.Content.ReadAsStringAsync();
            }

            // 6. Extraer RESULT_CODE y RESULT_TEXT (si vienen)
            try
            {
                var xml = XDocument.Parse(soapResponse);
                var resultCode = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "RESULT_CODE")?.Value;
                var resultText = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "RESULT_TEXT")?.Value;

                return $"✅ SAP RESULT: {resultCode} - {resultText}";
            }
            catch (Exception ex)
            {
                return $"⚠️ Error analizando respuesta de SAP: {ex.Message}\nRespuesta completa:\n{soapResponse}";
            }
        }

        // -------------------

        // -- GET api/<SAPController>/5
        // -- Se utiliza para leer datos de producción desde un archivo XML generado por SAP

        [HttpGet("{Centro}")]
        public async Task<string> Get(string Centro = "FO01")
        {
            // Configura la conexión a la base de datos
            SQLServerManager BBDD = BBDD_Config();

            // Ruta local del archivo XML descargado de SAP 
            string rutaXML = @"C:\Users\ZMES_GET_OFS_RESPONSE.xml";

            // Crea un nuevo documento XML donde cargaremos el archivo
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                // Verificamos si la carpeta que contiene el archivo existe
                var dirInfo = new DirectoryInfo(Path.GetDirectoryName(rutaXML));
                if (dirInfo.Exists)
                {
                    Console.WriteLine("La carpeta existe");
                }
                else
                {
                    Console.WriteLine("La carpeta NO existe");
                }

                // ====================== Como hemos descargado anteriormente el documento de SAP, aqui solo tenemos que leerlo ====================

                //Para funcionar leyendo de la apiSOAP
                //xmlDoc.LoadXml(soapResponse); // Cargar contenido de la respuesta de la peticion de datos a la api soap
                Console.WriteLine(rutaXML);
                
                //Para funcionar leyendo el archivo guardado
                xmlDoc.Load(rutaXML);

                // =================================================================================================================

                Console.WriteLine("XML cargado correctamente.");

            }
            catch (Exception ex)
            {
                // En caso de error al leer el archivo
                Console.WriteLine($"Error al cargar XML: {ex.Message}");
            }

            // Convertimos el contenido XML a formato JSON (más fácil de procesar en C#)
            string jsonString = JsonConvert.SerializeXmlNode(xmlDoc);

            // Convertimos el JSON a un objeto JObject para acceder fácilmente a sus propiedades
            JObject jsonObject = JObject.Parse(jsonString);

            // Creamos un objeto que representará información adicional para cada ítem
            JObject gmdixObject = new JObject();
            gmdixObject["version"] = new JArray("--");

            // Navegamos por el JSON para llegar a los items de órdenes de fabricación
            var items = jsonObject["soap-env:Envelope"]["soap-env:Body"]["n0:ZMES_GET_OFSResponse"]["ET_OFS"]["item"];

            int itemCount = items.Count();

            //Console.WriteLine("Contador de items:", itemCount);

            // Creamos una lista donde se guardarán las filas que insertaremos
            List<JObject> tablaSalida = new List<JObject>();
            HashSet<string> clavesUnicas = new HashSet<string>();
            int contadorID = 1;

            // Para evitar insertar combinaciones repetidas
            var combinacionesInsertadas = new HashSet<string>();
            int i_counter = 1;

            // Limpiamos la tabla de materiales antes de insertar nuevos datos

            await BBDD.EliminarTodosLosMateriales();

            // Recorremos cada item (orden de fabricación)
            for (int i = 0; i < itemCount; i++)
            {
                var etOFSObject = jsonObject["soap-env:Envelope"]["soap-env:Body"]["n0:ZMES_GET_OFSResponse"]["ET_OFS"]["item"][i];
                var material = etOFSObject["PLNBEZ"]; // Código del material
                var itemsPuestoTrabajo = etOFSObject["OPERACIONES"]["item"]; // Lista de puestos de trabajo
                int ordenFabricacion = Convert.ToInt32(etOFSObject["AUFNR"]);
                var Operacion = etOFSObject["OPERACIONES"];

                // Función local para procesar cada puesto de trabajo
                async Task ProcesarPuestoTrabajo(string puestoTrabajo)
                
                {
                    // Si es un reactor, consultamos su operación desde la base de datos
                    if (puestoTrabajo == "FO111001" || puestoTrabajo == "FO111002" || puestoTrabajo == "FO112001")
                    {
                        gmdixObject["operacion"] = await BBDD.ObtenerValor($"SELECT TOP 1 operacion FROM Reactores WHERE puestoTrabajo LIKE '{puestoTrabajo}'");

                    }
                    else
                    {
                        gmdixObject["operacion"] = " -- ";
                        
                    }

                    string materialStr = material?.ToString();
                    string operacionStr = gmdixObject["operacion"]?.ToString();

                    // Si el material es válido y no vacío
                    if (!string.IsNullOrEmpty(materialStr) && materialStr != "0")
                    {
                        // Solo para Puestos de trabajo de nuestro proyecto
                        if (puestoTrabajo == "FO111001" || puestoTrabajo == "FO111002" || puestoTrabajo == "FO112001")
                        {
                            // Si el material aún no está en la tabla, lo insertamos
                            bool materialExiste = await BBDD.ExisteMaterial(materialStr);

                            if (!materialExiste)
                            {
                                await BBDD.InsertarMaterial(i_counter, materialStr, operacionStr, puestoTrabajo);
                                i_counter++;
                            }
                        }

                        // Añadimos datos adicionales a la orden
                        gmdixObject["estado"] = await BBDD.OrdenesFabricacion(ordenFabricacion);
                        gmdixObject["nombreReactor"] = await BBDD.ObtenerListadoString($"SELECT nombreReactor FROM Reactores WHERE operacion LIKE '{gmdixObject["operacion"]}'");
                        gmdixObject["nombreReceta"] = await BBDD.ObtenerListadoString($"SELECT nombreReceta FROM Recetas WHERE material LIKE '{material}'");
                        etOFSObject["GMDix"] = gmdixObject; // Añadimos al JSON original
                    }
                    else {
                        // Si no hay material, rellenamos con valores vacíos
                        gmdixObject["estado"] = "Vacio";
                        gmdixObject["nombreReactor"] = "Vacio";
                        gmdixObject["nombreReceta"] = "Vacio";
                        etOFSObject["GMDix"] = gmdixObject;
                    }
                                        
                }
                // Si hay varios puestos de trabajo (array), procesamos todos
                if (itemsPuestoTrabajo is JArray itemsArray)
                {
                    foreach (var item in itemsArray)
                    {
                        await ProcesarPuestoTrabajo(item["ARBPL"].ToString());
                    }
                }
                else
                {
                    // Si es solo uno, también lo procesamos
                    await ProcesarPuestoTrabajo(itemsPuestoTrabajo["ARBPL"].ToString());
                }

                // --- Segunda parte: evitar combinaciones repetidas ---
                var Aux_etOFSObject = jsonObject["soap-env:Envelope"]["soap-env:Body"]["n0:ZMES_GET_OFSResponse"]["ET_OFS"]["item"][i];
                var Aux_material = Aux_etOFSObject["PLNBEZ"]?.ToString();
                var Aux_operacionObj = Aux_etOFSObject["OPERACIONES"];
                var Aux_itemsPuestoTrabajo = Aux_operacionObj["item"];

                string Aux_nombreOperacion = Aux_operacionObj["VORNR"]?.ToString() ?? "--"; // Reemplaza si "VORNR" no es el correcto

                if (Aux_itemsPuestoTrabajo is JArray Aux_puestoArray)
                {
                    foreach (var Aux_item in Aux_puestoArray)
                    {
                        string Aux_puesto = Aux_item["ARBPL"]?.ToString() ?? "--";
                        string Aux_clave = $"{Aux_material}|{Aux_nombreOperacion}|{Aux_puesto}";

                        if (!clavesUnicas.Contains(Aux_clave))
                        {

                            if (Aux_puesto == "FO111001" || Aux_puesto == "FO111002" || Aux_puesto == "FO112001") 
                            {

                                clavesUnicas.Add(Aux_clave);
                                var Aux_fila = new JObject
                                {
                                    ["ID"] = contadorID++,
                                    ["Nombre"] = Aux_material,
                                    ["Operacion"] = gmdixObject["operacion"],
                                    ["PuestoTrabajo"] = Aux_puesto,
                                    ["MateriaPrima"] = Aux_etOFSObject["MAKTX"]?.ToString()
                                };
                                tablaSalida.Add(Aux_fila);
                            }
                        }
                    }
                }
                else
                {

                }
            }
            // Convertimos el JSON final (ya modificado con GMDix) en string
            string jsonResult = jsonObject.ToString();

            return jsonResult;

        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Este endpoint se accede mediante: GET api/SAPController/obtenerVersionesReceta?receta=NombreReceta

        [HttpGet("obtenerVersionesReceta")]
        public async Task<String> ObtenerVersionesReceta([FromQuery] string receta)
        {
            // 1️. Configuramos y abrimos la conexión con la base de datos SQL Server
            SQLServerManager BBDD = BBDD_Config();

            // 2️. Ejecutamos una consulta SQL para obtener las versiones de la receta solicitada.
            //    - La consulta busca todas las filas en la tabla "Recetas" donde el nombre coincida
            //    - Devuelve solo el campo "version" como una lista de enteros
            var versiones = await BBDD.ObtenerListadoInt(
                $"SELECT version FROM Recetas WHERE nombreReceta LIKE '{receta}'"
            );

            // 3️. Convertimos esa lista en un objeto JSON como:
            //    { "versiones": [1, 2, 3] }
            return JsonConvert.SerializeObject(new { versiones });
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Este método responde a solicitudes HTTP POST (aunque debería llamarse "Post", no "Put", por convención)

        [HttpPost]
        public async Task<IActionResult> Put(JsonElement Datos)
        {
            // 1️. Se configura la conexión a la base de datos
            SQLServerManager BBDD = BBDD_Config();

            // 2️.  Se convierte el objeto JSON recibido (en tipo JsonElement) a un JObject (estructura de datos manejable)
            JObject DatosNuevos = JsonConvert.DeserializeObject<JObject>(Datos.GetRawText());

            // ✅ Aquí podrías mostrar los datos por consola para depuración (está comentado por ahora)
            // Console.WriteLine("Estos son los Datos");
            // Console.WriteLine(Datos);

            // 3️.  Se llama al método para actualizar el estado en la base de datos usando los datos recibidos
            await BBDD.ActualizarEstado(DatosNuevos);

            // 4️.  Devuelve una respuesta HTTP 200 OK indicando que todo fue exitoso
            return Ok();
        }

        // ---------------------------------------------------------------------------------------------------------------------------

    }
}
