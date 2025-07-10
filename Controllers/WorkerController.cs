using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using API_SAP.Clases;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using GestionRecetas.Models;
using GestionRecetas.Clases;
using GestionRecetas.Datos;
using System.Diagnostics;
using System.Data.SqlClient;


namespace API_SAP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerController : Controller
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

        // Esta función responde a una petición GET en la ruta "AlgunaLanzada/{nombreReactor}"
        // y devuelve información sobre si hay alguna orden lanzada para un reactor específico.
        // Si no se pasa un nombre, usa "RC01" por defecto.

        [HttpGet("AlgunaLanzada/{nombreReactor}")]
        public async Task<IActionResult> Get(string nombreReactor = "RC01")
        {
            // Creamos la conexión/configuración para acceder a la base de datos
            SQLServerManager BBDD = BBDD_Config();

            // Consultamos en la base de datos si hay alguna orden lanzada para el reactor dado.
            // El resultado puede venir en diferentes formatos, como lista de diccionarios o JSON.
            var algunaLanzada = await BBDD.RevisarLazadas(nombreReactor);

            // Este código está comentado, sirve para imprimir en consola el resultado recibido.
            //Console.WriteLine($"Alguna Lanzada: {JsonConvert.SerializeObject(algunaLanzada)}");

            // Aquí validamos si el resultado es una lista de diccionarios con la clave "Existe"
            // y si el valor de "Existe" es false, entonces respondemos con false.
            if (algunaLanzada is List<Dictionary<string, object>> listaDict &&
                listaDict.Count > 0 &&
                listaDict[0].ContainsKey("Existe") &&
                listaDict[0]["Existe"] is bool existeValor &&
                !existeValor)
            {
                // No hay órdenes lanzadas, devolvemos false
                return Ok(false);
            }

            // Si el resultado es nulo o es un arreglo JSON vacío, devolvemos false
            if (algunaLanzada == null || (algunaLanzada is JArray ja && !ja.Any()))
            {
                return Ok(false);
            }

            // Ahora aseguramos que el resultado sea un JArray (arreglo JSON).
            // Si ya es JArray, lo usamos; si no, lo convertimos en uno.
            JArray jsonArray;
            if (algunaLanzada is JArray)
            {
                jsonArray = (JArray)algunaLanzada;
            }
            else
            {
                jsonArray = JArray.FromObject(algunaLanzada); // Convierte listas u objetos en JArray
            }

            // Llamamos a otra función que transforma este JSON para dejarlo en el formato que queremos.
            JArray resultado = await JSON.TransformarJson(jsonArray,BBDD);

            // Convertimos el resultado final a una cadena JSON bien formateada.
            var jsonString = JsonConvert.SerializeObject(resultado, Formatting.Indented);

            // Este código también está comentado, sirve para imprimir el JSON final en consola.
            //Console.WriteLine($"Este es mi Json Result: {jsonString}");

            // Devolvemos el JSON como respuesta con un código HTTP 200 (OK).
            return Ok(jsonString);
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        public class OFRequest
        {
            public string OF { get; set; }
            public string nombreEtapa { get; set; }
            public string numeroEtapa { get; set; }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Esta función responde a una petición POST en la ruta "ActualizarOF".
        // Recibe datos en el cuerpo de la petición para actualizar una orden de fabricación (OF).

        [HttpPost("ActualizarOF")]
        public async Task<IActionResult> Post([FromBody] OFRequest request)
        {
            // Primero verificamos que el objeto request no sea nulo.
            // Esto puede pasar si el JSON enviado no tiene el formato correcto.
            if (request == null)
            {
                // Si es nulo, mostramos un mensaje en consola y devolvemos un error 400 (Bad Request)
                // con un mensaje indicando que el modelo no pudo deserializarse.
                Console.WriteLine("⚠️ request = null");
                return BadRequest("El modelo no se pudo deserializar.");
            }

            // Los siguientes Console.WriteLine están comentados, pero sirven para depurar e imprimir
            // la información recibida por el request:
            // Console.WriteLine($"OF: {request.OF}");
            // Console.WriteLine($"nombreEtapa: {request.nombreEtapa}");
            // Console.WriteLine($"numeroEtapa: {request.numeroEtapa}");

            // Creamos la conexión/configuración para acceder a la base de datos.
            SQLServerManager BBDD = BBDD_Config();

            // Llamamos a la función que actualiza la orden de fabricación en la base de datos,
            // pasando los datos recibidos en el request.
            await BBDD.ActualizarOF(request.OF, request.nombreEtapa, request.numeroEtapa);

            // Devolvemos un OK (código HTTP 200) para indicar que la operación fue exitosa.
            return Ok();
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        public class OFRequestFinalizar
        {
            public string OF { get; set; } 
            public string estado { get; set; }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Esta función responde a una petición POST en la ruta "FinalizarOF".
        // Se usa para marcar una orden de fabricación (OF) como finalizada con un estado específico.

        [HttpPost("FinalizarOF")]
        public async Task<IActionResult> PostFinalizarOF([FromBody] OFRequestFinalizar request)
        {
            // Creamos la conexión/configuración para acceder a la base de datos.
            SQLServerManager BBDD = BBDD_Config();

            // Llamamos a la función que marca la orden de fabricación como finalizada,
            // pasando el número de orden (OF) y el estado recibido en el request.
            await BBDD.FinalizarOF(request.OF, request.estado);

            // Devolvemos un OK (código HTTP 200) para indicar que la operación fue exitosa.
            return Ok();
        }

        // ---------------------------------------------------------------------------------------------------------------------------

    }
}
