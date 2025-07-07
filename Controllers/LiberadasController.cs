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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API_SAP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiberadasController : ControllerBase
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


        [HttpGet("SAP/{Centro}")]
        public async Task<string> Get_SAP(string Centro = "FO01")
        {
            //Console.WriteLine("[$DEBUG]:INICIAO API SAP");

            string urlServicio = "http://SAPPRD.samca.net:8001/sap/bc/srt/rfc/sap/zmes_ws_ofs/010/zmes_ws_ofs/zmes_bn_ofs"; //datos

            string xmlSolicitud = @$"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:sap-com:document:sap:rfc:functions"">
                    <soapenv:Header/>
                    <soapenv:Body>
                        <urn:ZMES_GET_OFS>
                            <IV_CENTRO>{Centro}</IV_CENTRO>
                            <!--Optional:-->
                            <IV_MATERIAL></IV_MATERIAL>
                            <!--Optional:-->
                            <IV_PUESTO></IV_PUESTO>
                        </urn:ZMES_GET_OFS>
                    </soapenv:Body>
                </soapenv:Envelope>";

            //Console.WriteLine(xmlSolicitud);
            string soapResponse;
            string rutaXML = @"C:\Users\ZMES_GET_OFS_RESPONSE_TEST.xml";

            //// Configurar cliente HttpClient
            //using (HttpClient httpClient = new HttpClient())
            //{
            //    httpClient.DefaultRequestHeaders.Add("SOAPAction", "urn:sap-com:document:sap:rfc:functions:ZMES_GET_OFS");
            //    httpClient.DefaultRequestHeaders.Add("Accept", "text/xml");

            //    // Configurar el contenido de la solicitud SOAP
            //    var content = new StringContent(xmlSolicitud, Encoding.UTF8, "text/xml");

            //    // Enviar solicitud POST al servicio web SOAP
            //    HttpResponseMessage response = await httpClient.PostAsync(urlServicio, content);

            //    // Leer la respuesta SOAP
            //    soapResponse = await response.Content.ReadAsStringAsync();

            //    // Mostrar la respuesta en la consola
            //    //Console.WriteLine("Respuesta de la API SOAP:");
            //    //Console.WriteLine(soapResponse);

            //    try
            //    {
            //        rutaXML = @"C:\ZMES_GET_OFS_RESPONSE.xml";

            //        // Guarda el contenido XML en el archivo
            //        System.IO.File.WriteAllText(rutaXML, soapResponse);

            //        Console.WriteLine($"Archivo guardado correctamente en: {rutaXML}");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Error al guardar el archivo: {ex.Message}");
            //    }
            //}
            //Console.WriteLine("[$DEBUG]:FIN API SAP");

            return null;
        }




        // GET api/<SAPController>/5
        [HttpGet("{Centro}")]
        public async Task<string> Get(string Centro = "FO01")
        {
            //Console.WriteLine("Debug 10");

            SQLServerManager BBDD = BBDD_Config(); 
            //Console.WriteLine("[$DEBUG]:INICIAO API RELOAD");

            /*
            string urlServicio = "http://SAPPRD.samca.net:8001/sap/bc/srt/rfc/sap/zmes_ws_ofs/010/zmes_ws_ofs/zmes_bn_ofs"; //datos

            string xmlSolicitud = @$"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:sap-com:document:sap:rfc:functions"">
                    <soapenv:Header/>
                    <soapenv:Body>
                        <urn:ZMES_GET_OFS>
                            <IV_CENTRO>{Centro}</IV_CENTRO>
                            <!--Optional:-->
                            <IV_MATERIAL></IV_MATERIAL>
                            <!--Optional:-->
                            <IV_PUESTO></IV_PUESTO>
                        </urn:ZMES_GET_OFS>
                    </soapenv:Body>
                </soapenv:Envelope>";

            //Console.WriteLine(xmlSolicitud);
            string soapResponse;
            */
            string rutaXML = @"C:\Users\ZMES_GET_OFS_RESPONSE_TEST.xml";

            //// Configurar cliente HttpClient
            //using (HttpClient httpClient = new HttpClient())
            //{
            //    httpClient.DefaultRequestHeaders.Add("SOAPAction", "urn:sap-com:document:sap:rfc:functions:ZMES_GET_OFS");
            //    httpClient.DefaultRequestHeaders.Add("Accept", "text/xml");

            //    // Configurar el contenido de la solicitud SOAP
            //    var content = new StringContent(xmlSolicitud, Encoding.UTF8, "text/xml");

            //    // Enviar solicitud POST al servicio web SOAP
            //    HttpResponseMessage response = await httpClient.PostAsync(urlServicio, content);

            //    // Leer la respuesta SOAP
            //    soapResponse = await response.Content.ReadAsStringAsync();

            //    // Mostrar la respuesta en la consola
            //    //Console.WriteLine("Respuesta de la API SOAP:");
            //    //Console.WriteLine(soapResponse);

            //    try
            //    {
            //        rutaXML = @"C:\ZMES_GET_OFS_RESPONSE.xml";

            //        // Guarda el contenido XML en el archivo
            //        System.IO.File.WriteAllText(rutaXML, soapResponse);

            //        Console.WriteLine($"Archivo guardado correctamente en: {rutaXML}");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Error al guardar el archivo: {ex.Message}");
            //    }
            //}

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                var dirInfo = new DirectoryInfo(Path.GetDirectoryName(rutaXML));
                if (dirInfo.Exists)
                {
                    Console.WriteLine("La carpeta existe");
                }
                else
                {
                    Console.WriteLine("La carpeta NO existe");
                }
                // =================================================================================================================

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
                Console.WriteLine($"Error al cargar XML: {ex.Message}");
            }

            // Convertir XML a JSON
            string jsonString = JsonConvert.SerializeXmlNode(xmlDoc);

            // Convertir JSON a JObject para manipulación
            JObject jsonObject = JObject.Parse(jsonString);

            // Crear el objeto GMDix
            JObject gmdixObject = new JObject();
            gmdixObject["version"] = new JArray("--");

            // Obtener la lista de "item"
            var items = jsonObject["soap-env:Envelope"]["soap-env:Body"]["n0:ZMES_GET_OFSResponse"]["ET_OFS"]["item"];

            int itemCount = items.Count();

            //Console.WriteLine("Contador de items:", itemCount);

            // ****** 
            List<JObject> tablaSalida = new List<JObject>();
            HashSet<string> clavesUnicas = new HashSet<string>();
            int contadorID = 1;

            // Evitar combinadciones repetidas en tabvla materias
            var combinacionesInsertadas = new HashSet<string>();
            int i_counter = 1;
            // ******

            // Eliminamos contenido de Tabla Materias

            await BBDD.EliminarTodosLosMateriales();



            // Agregar el objeto GMDix a cada elemento de la lista "item"
            for (int i = 0; i < itemCount; i++)
            {
                var etOFSObject = jsonObject["soap-env:Envelope"]["soap-env:Body"]["n0:ZMES_GET_OFSResponse"]["ET_OFS"]["item"][i];
                var material = etOFSObject["PLNBEZ"];
                var itemsPuestoTrabajo = etOFSObject["OPERACIONES"]["item"];
                int ordenFabricacion = Convert.ToInt32(etOFSObject["AUFNR"]);

                var Operacion = etOFSObject["OPERACIONES"];

                //Console.WriteLine("Operacion:", itemsPuestoTrabajo);

                async Task ProcesarPuestoTrabajo(string puestoTrabajo)

                
                {                  
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


                    if (!string.IsNullOrEmpty(materialStr) && materialStr != "0")
                    {
                        // Rellleno tabla materias para saber que material tengo e
                        if (puestoTrabajo == "FO111001" || puestoTrabajo == "FO111002" || puestoTrabajo == "FO112001")
                        {

                            // Insertamos valores en BBDD Materias

                            bool materialExiste = await BBDD.ExisteMaterial(materialStr);

                            if (!materialExiste)
                            {
                                await BBDD.InsertarMaterial(i_counter, materialStr, operacionStr, puestoTrabajo);
                                i_counter++;
                            }

                        }

                        gmdixObject["estado"] = await BBDD.OrdenesFabricacion(ordenFabricacion);
                        gmdixObject["nombreReactor"] = await BBDD.ObtenerListadoString($"SELECT nombreReactor FROM Reactores WHERE operacion LIKE '{gmdixObject["operacion"]}'");
                        //gmdixObject["nombreReceta"] = await BBDD.ObtenerListadoString($"SELECT nombreReceta FROM Recetas WHERE material LIKE '{material}' AND operacion LIKE '{gmdixObject["operacion"]}'");
                        gmdixObject["nombreReceta"] = await BBDD.ObtenerListadoString($"SELECT nombreReceta FROM Recetas WHERE material LIKE '{material}'");
                        etOFSObject["GMDix"] = gmdixObject;


                    }
                    else {
                        
                        gmdixObject["estado"] = "Vacio";
                        gmdixObject["nombreReactor"] = "Vacio";
                        gmdixObject["nombreReceta"] = "Vacio";
                        etOFSObject["GMDix"] = gmdixObject;
                    }
                                        
                }


                if (itemsPuestoTrabajo is JArray itemsArray)
                {
                    foreach (var item in itemsArray)
                    {
                        await ProcesarPuestoTrabajo(item["ARBPL"].ToString());
                    }
                }
                else
                {
                    await ProcesarPuestoTrabajo(itemsPuestoTrabajo["ARBPL"].ToString());
                }


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

            //Console.WriteLine(JsonConvert.SerializeObject(tablaSalida));

            // Convertir el objeto modificado a JSON
            string jsonResult = jsonObject.ToString();

            //Console.WriteLine("📤 JSON final generado:");
            //Console.WriteLine(jsonResult);

            //Console.WriteLine("[$DEBUG]:FIN API RELOAD");
            return jsonResult;

        }

        [HttpGet("obtenerVersionesReceta")]
        public async Task<String> ObtenerVersionesReceta([FromQuery] string receta)
        {
            SQLServerManager BBDD = BBDD_Config();

            var versiones = await BBDD.ObtenerListadoInt($"SELECT version FROM Recetas WHERE nombreReceta LIKE '{receta}'");

            return JsonConvert.SerializeObject(new { versiones });
        }




        [HttpPost]
        public async Task<IActionResult> Put(JsonElement Datos)
        {
            SQLServerManager BBDD = BBDD_Config();

            JObject DatosNuevos = JsonConvert.DeserializeObject<JObject>(Datos.GetRawText());

            //Console.WriteLine("Estos son los Datos");
            //Console.WriteLine(Datos);
            
            await BBDD.ActualizarEstado(DatosNuevos);

            return Ok();
        }
    }
}
