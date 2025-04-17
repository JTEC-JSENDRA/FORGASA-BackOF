using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using GestionRecetas.Models;

namespace GestionRecetas.Clases
{
    public class JSON
    {

        private readonly JsonElement jsonData;

        public static JArray TransformarJson(JArray jsonArray)
        {
            JArray recetaFinal = new JArray();

            foreach (JObject receta in jsonArray)
            {
                // Convertimos la receta en el formato esperado
                JArray recetaCabecera = new JArray
            {
                new JObject
                {
                    ["id"] = 0,
                    ["ordenFabricacion"] = receta["ordenFabricacion"],
                    ["nombreReceta"] = receta["nombreReceta"],
                    ["version"] = receta["version"],
                    ["nombreReactor"] = receta["nombreReactor"],
                    ["numeroEtapas"] = receta["numeroEtapas"],
                    ["creada"] = receta["creada"],
                    ["modificada"] = receta["modificada"],
                    ["eliminada"] = receta["eliminada"]
                }
            };

                // Agregamos la cabecera como primer elemento del array final
                recetaFinal.Add(recetaCabecera);

                // Simulación de etapas (Aquí debes poner las etapas reales desde la base de datos)
                for (int i = 1; i <= (int)receta["numeroEtapas"]; i++)
                {
                    JArray etapa = new JArray();

                    // Cabecera de la etapa
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["n_Etapa"] = i,
                        ["nombre"] = i == 1 ? "Carga" : "Espera",
                        ["etapaActiva"] = 1
                    }
                });

                    // Procesos de ejemplo (esto debería venir de la base de datos)
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Carga_Solidos_1",
                        ["consigna"] = "Cantidad",
                        ["valor"] = 1.0,
                        ["procesoActivo"] = false
                    },
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Carga_Solidos_1",
                        ["consigna"] = "Velocidad_Vibracion",
                        ["valor"] = 2.0
                    },
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Carga_Solidos_2",
                        ["consigna"] = "Cantidad",
                        ["valor"] = 3.0,
                        ["procesoActivo"] = true
                    },
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Carga_Solidos_2",
                        ["consigna"] = "Velocidad_Vibracion",
                        ["valor"] = 4.0
                    },
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Carga_Solidos_3",
                        ["consigna"] = "Cantidad",
                        ["valor"] = 5.0,
                        ["procesoActivo"] = true
                    },
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Carga_Solidos_3",
                        ["consigna"] = "Velocidad_Vibracion",
                        ["valor"] = 6.0
                    },
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Carga_Agua_Descal",
                        ["consigna"] = "Cantidad",
                        ["valor"] = 7.0,
                        ["procesoActivo"] = false
                    }
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Carga_Agua_Recup",
                        ["consigna"] = "Cantidad",
                        ["valor"] = 8.0,
                        ["procesoActivo"] = true
                    }
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Carga_Antiespumante",
                        ["consigna"] = "Cantidad",
                        ["valor"] = 9.0,
                        ["procesoActivo"] = false
                    }
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Carga_Ligno",
                        ["consigna"] = "Cantidad",
                        ["valor"] = 10.0,
                        ["procesoActivo"] = false
                    }
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Carga_Potasa",
                        ["consigna"] = "Cantidad",
                        ["valor"] = 11.0,
                        ["procesoActivo"] = true
                    }
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Espera",
                        ["consigna"] = "Tiempo",
                        ["valor"] = 12,
                        ["procesoActivo"] = false
                    }
                });
                    // Procesos secundarios vacíos
                    etapa.Add(new JArray // Agitación
                {
                    new JObject
                {
                        ["id"] = 0,
                        ["tipo"] = "Agitacion",
                        ["consigna"] = "Modo",
                        ["valor"] = 13,
                        ["procesoActivo"] = true
                },
                    new JObject
                {
                        ["id"] = 0,
                        ["tipo"] = "Agitacion",
                        ["consigna"] = "Intermitencia",
                        ["valor"] = true
                },
                    new JObject
                {
                        ["id"] = 0,
                        ["tipo"] = "Agitacion",
                        ["consigna"] = "Velocidad",
                        ["valor"] = 15.0
                }, 
                    new JObject
                {
                        ["id"] = 0,
                        ["tipo"] = "Agitacion",
                        ["consigna"] = "Temporizado",
                        ["valor"] = 16
                },
                    new JObject
                {
                        ["id"] = 0,
                        ["tipo"] = "Agitacion",
                        ["consigna"] = "Tiempo_ON",
                        ["valor"] = 17
                },
                    new JObject
                {
                        ["id"] = 0,
                        ["tipo"] = "Agitacion",
                        ["consigna"] = "Tiempo_OFF",
                        ["valor"] = 18
                }    
                }); 
                    etapa.Add(new JArray // Temperatura
                {
                    new JObject
                {
                        ["id"] = 0,
                        ["tipo"] = "Temperatura",
                        ["consigna"] = "Temperatura",
                        ["valor"] = 19,
                        ["procesoActivo"] = false
                }
                }); 

                    // Agregamos la etapa completa al array final
                    recetaFinal.Add(etapa);
                }
            }

            return recetaFinal;
        }

        public JSON(JsonElement jsonData)
        {
            this.jsonData = jsonData;
        }

        public CabeceraReceta ObtenerCabeceraReceta()
        {
            CabeceraReceta Cabecera = new CabeceraReceta();
            string jsonString = jsonData.ToString();

            JArray jsonArray = JArray.Parse(jsonString);

            // Acceder a la cabecera de la receta
            JObject primerObjeto = (JObject)jsonArray[0][0];
            Cabecera.ID = primerObjeto.Value<int>("id");
            Cabecera.NombreReceta = primerObjeto.Value<string>("nombreReceta");
            Cabecera.NombreReactor = primerObjeto.Value<string>("nombreReactor");
            Cabecera.NumeroEtapas = primerObjeto.Value<short>("numeroEtapas");

            if (primerObjeto["modificada"].Type != JTokenType.Null)
            {
                Cabecera.Creada = primerObjeto.Value<DateTime>("creada");
            }

            if (primerObjeto["modificada"].Type != JTokenType.Null)
            {
                Cabecera.Modificada = primerObjeto.Value<DateTime>("modificada");
            }

            if (primerObjeto["eliminada"].Type != JTokenType.Null)
            {
                Cabecera.Eliminada = primerObjeto.Value<DateTime>("eliminada");
            }

            return Cabecera;
        }

        public CabeceraEtapa ObtenerCabeceraEtapa(short NumeroEtapa)
        {
            CabeceraEtapa Cabecera = new CabeceraEtapa();
            string jsonString = jsonData.ToString();

            JArray jsonArray = JArray.Parse(jsonString);

            // Acceder ala cabecera de la etapa
            JObject primerObjeto = (JObject)jsonArray[NumeroEtapa][0][0];
            Cabecera.ID = primerObjeto.Value<int>("id");
            Cabecera.EtapaActiva = primerObjeto.Value<short>("etapaActiva");
            Cabecera.N_Etapa = primerObjeto.Value<short>("n_Etapa");
            Cabecera.Nombre = primerObjeto.Value<string>("nombre");

            return Cabecera;
        }

        public List<CsgProceso1> ObtenerConsignasProceso(int NumeroEtapa, int NumeroProceso)
        {
            List<CsgProceso1> ListaConsignas = new List<CsgProceso1>();
            string jsonString = jsonData.ToString();

            JArray jsonArray = JArray.Parse(jsonString);

            // Acceder a las consignas individuales de cada proceso según la etapa
            JArray proceso = (JArray)jsonArray[NumeroEtapa][NumeroProceso];
            int numeroConsignas = proceso.Count;

            //Console.WriteLine($"Numero etapa: {NumeroEtapa}  - NumeroProceso: {NumeroProceso}  - Numero consignas: {numeroConsignas}");

            for (int NumeroConsigna = 0; NumeroConsigna < numeroConsignas; NumeroConsigna++)
            {
                CsgProceso1 Consigna = new CsgProceso1();
                JObject primerObjeto = (JObject)jsonArray[NumeroEtapa][NumeroProceso][NumeroConsigna];

                Consigna.ID = primerObjeto.Value<int>("id");
                Consigna.Tipo = primerObjeto.Value<string>("tipo");
                Consigna.Consigna = primerObjeto.Value<string>("consigna");
                Consigna.Valor = primerObjeto.Value<string>("valor");

                ListaConsignas.Add(Consigna);
            }

            return ListaConsignas;
        }

        public List<int[]> GetListadosID(int NumeroProcesos, int NumeroEtapas)
        {
            List<int[]> Listado = new List<int[]>();

            string jsonString = jsonData.ToString();
            JArray jsonArray = JArray.Parse(jsonString);

            int[] IDsEtapas = new int[NumeroEtapas];
            int[] IDsConsignas;
            int NumeroConsignas;
            int Indice;

            //Se obtiene el array de IDs de las etapas
            for (int Etapa = 0; Etapa < NumeroEtapas; Etapa++)
            {
                JObject etapa = (JObject)jsonArray[Etapa + 1][0][0];
                IDsEtapas[Etapa] = etapa.Value<int>("id");
            }
            Listado.Add(IDsEtapas);

            //Se obtienen los IDs de las distintas consignas
            for (int Proceso = 1; Proceso < NumeroProcesos; Proceso++)
            {
                Indice = 0;
                NumeroConsignas = 0;

                for (int Etapa = 1; Etapa <= NumeroEtapas; Etapa++)
                {
                    JArray proceso = (JArray)jsonArray[Etapa][Proceso];
                    NumeroConsignas = proceso.Count + NumeroConsignas;
                }
                Console.WriteLine($"Numero de consignas del proceso {Proceso}: {NumeroConsignas}");
                IDsConsignas = new int[NumeroConsignas];

                for (int Etapa = 1; Etapa <= NumeroEtapas; Etapa++)
                {
                    JArray proceso = (JArray)jsonArray[Etapa][Proceso];
                    int NumeroConsignasProceso = proceso.Count;

                    for (int NumeroConsigna = 0; NumeroConsigna < NumeroConsignasProceso; NumeroConsigna++)
                    {
                        JObject consigna = (JObject)jsonArray[Etapa][Proceso][NumeroConsigna];
                        IDsConsignas[Indice] = consigna.Value<int>("id");
                        Console.WriteLine($"Etapa: {Etapa}  - ID Consigna: {IDsConsignas[Indice]}");
                        Indice++;
                    }
                }
                Listado.Add(IDsConsignas);
            }

            return Listado;

        }
    }
}
