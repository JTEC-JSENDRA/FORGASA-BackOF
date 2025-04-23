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

        [HttpGet("AlgunaLanzada/{nombreReactor}")]
        public async Task<IActionResult> Get(string nombreReactor = "RC01")
        {
            SQLServerManager BBDD = BBDD_Config();

            var algunaLanzada = await BBDD.RevisarLazadas(nombreReactor);

            // Si algunaLanzada es una lista de diccionarios con un "Existe": false, devolver false
            if (algunaLanzada is List<Dictionary<string, object>> listaDict &&
                listaDict.Count > 0 &&
                listaDict[0].ContainsKey("Existe") &&
                listaDict[0]["Existe"] is bool existeValor &&
                !existeValor)
            {
                return Ok(false);
            }

            // Si algunaLanzada es null o vacío, retornar false
            if (algunaLanzada == null || (algunaLanzada is JArray ja && !ja.Any()))
            {
                return Ok(false);
            }

            // Si ya es un JArray, úsalo directamente
            JArray jsonArray;
            if (algunaLanzada is JArray)
            {
                jsonArray = (JArray)algunaLanzada;
            }
            else
            {
                jsonArray = JArray.FromObject(algunaLanzada); // Convierte listas u objetos en JArray
            }

            // Llamamos a la función para transformar los datos
            JArray resultado = JSON.TransformarJson(jsonArray);

            //Console.WriteLine($"Tipo de resultado: {resultado.GetType()}");

            var jsonString = JsonConvert.SerializeObject(resultado, Formatting.Indented);
            //Console.WriteLine($"JSON String: {jsonString}");

            return Ok(jsonString);
        }

        [HttpPost("ActualizarOF")]
        ///api/Worker/ActualizarOF?OF=1259353&nombreEtapa=Primera&numeroEtapa=4/5
        public async Task<IActionResult> Post([FromBody] OFRequest request)
        {
            if (request == null)
            {
                Console.WriteLine("⚠️ request = null");
                return BadRequest("El modelo no se pudo deserializar.");
            }

            SQLServerManager BBDD = BBDD_Config();
            await BBDD.ActualizarOF(request.OF, request.nombreEtapa, request.numeroEtapa);
            return Ok();
        }

        public class OFRequest
        {
            public string OF { get; set; }
            public string nombreEtapa { get; set; }
            public string numeroEtapa { get; set; }
        }
    }
}
