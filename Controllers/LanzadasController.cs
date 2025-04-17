using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using API_SAP.Clases;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        // GET: api/<LanzadasController>
        [HttpGet]
        public async Task<string> Get()
        {
            SQLServerManager BBDD = BBDD_Config();

            List<JObject> jsonObjectList = new List<JObject>();

            jsonObjectList = await BBDD.ListadoLanzadas();

            string jsonResult = JsonConvert.SerializeObject(jsonObjectList, Formatting.Indented);

            return jsonResult;
        }


    }
}
