using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using GestionRecetas.Models;
using System.Threading.Tasks;
using API_SAP.Clases;


namespace GestionRecetas.Clases
{
    public class JSON
    {

        private readonly JsonElement jsonData;

        public static async Task<JArray> TransformarJson(JArray jsonArray, SQLServerManager BBDD)
        {
            JArray recetaFinal = new JArray();

            foreach (JObject receta in jsonArray)
            {
                // 🔹 LOG: Mostrar receta original antes de procesarla
                //Console.WriteLine("Receta original:" + receta.ToString());
                //Console.WriteLine("Receta original: " + recetaFinal.ToString());

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

                // SACAMAOS ID RECETA
                string nombreReceta = receta["nombreReceta"]?.ToString();
                //Console.WriteLine($"Nombre Receta: {nombreReceta}");
                int? ID_Receta = await BBDD.ObtenerIDReceta(nombreReceta);
                //Console.WriteLine($"ID_RECETA: {ID_Receta}");

                // Simulación de etapas (Aquí debes poner las etapas reales desde la base de datos)
                for (int i = 1; i <= (int)receta["numeroEtapas"]; i++)
                {
                    JArray etapa = new JArray();

                    //Console.WriteLine("      ");
                    //Console.WriteLine("================= NUEVA ETAPA ========================");
                    //Console.WriteLine("      ");
                    // - - - - - - - - - - -
                    // i = N_Etapa
                    // ID_Receta -> tengo el nombte de la receta tengo que hacer un select y luego coger el id dependiendo del nombre
                    // Con N_Etapa y ID_Receta ir por Materia prima para ver si hay algun valor y asignarlo si no se encuentra meter en proceso activo un falte

                    decimal? valorCargaSolidos1 = 0;
                    decimal? valorCargaSolidos2 = 0;
                    decimal? valorCargaSolidos3 = 0;
                    decimal? valorCargaAgua = 0;
                    decimal? valorCargaAguaRecu = 0;
                    decimal? valorCargaAntiEs = 0;
                    decimal? valorCargaLigno = 0;
                    decimal? valorCargaPotasa = 0;

                    bool ProcesoActivo_CS1 = false;
                    bool ProcesoActivo_CS2 = false;
                    bool ProcesoActivo_CS3 = false;
                    bool ProcesoActivo_Agua = false;
                    bool ProcesoActivo_AguaRecup = false;
                    bool ProcesoActivo_AntiEs = false;
                    bool ProcesoActivo_Ligno = false;
                    bool ProcesoActivo_Potasa = false;

                    //BBDD.ExtraerValorMMPP (int ID_Receta, int N_Etapa, decimal MMPP)
                    // !!!!! FALTA AÑADIR ID ETAPA !!!!!!

                    // - - LC70 -> id MMPP 1
                    valorCargaSolidos1 = await BBDD.ExtraerValorMMPP(ID_Receta, i, 1);
                    if (valorCargaSolidos1 != null) { ProcesoActivo_CS1 = true;} else { valorCargaSolidos1 = 0; ProcesoActivo_CS1 = false; }
                    // - - LC80 -> id MMPP 2
                    valorCargaSolidos2 = await BBDD.ExtraerValorMMPP(ID_Receta, i, 2);
                    if (valorCargaSolidos2 != null) { ProcesoActivo_CS2 = true;} else { valorCargaSolidos2 = 0; ProcesoActivo_CS2 = false; }
                    // - - HL26(10-16)(0-0-8) -> id MMPP 3
                    valorCargaSolidos3 = await BBDD.ExtraerValorMMPP(ID_Receta, i, 3);
                    if (valorCargaSolidos3 != null) { ProcesoActivo_CS3 = true;} else { valorCargaSolidos3 = 0; ProcesoActivo_CS3 = false; }
                    // - - AGUA -> id MMPP 4
                    valorCargaAgua = await BBDD.ExtraerValorMMPP(ID_Receta, i, 4);
                    if (valorCargaAgua != null) { ProcesoActivo_Agua = true;} else { valorCargaAgua = 0; ProcesoActivo_Agua = false; }
                    // - - AGUA RECUPERADA -> id MMPP 5
                    valorCargaAguaRecu = await BBDD.ExtraerValorMMPP(ID_Receta, i, 5);
                    if (valorCargaAguaRecu != null) { ProcesoActivo_AguaRecup = true;} else { valorCargaAguaRecu = 0; ProcesoActivo_AguaRecup = false; }
                    // - - HL PRUEBAS -> id MMPP 6
                    valorCargaAntiEs = await BBDD.ExtraerValorMMPP(ID_Receta, i, 6);
                    if (valorCargaAntiEs != null) { ProcesoActivo_AntiEs = true;} else { valorCargaAntiEs = 0; ProcesoActivo_AntiEs = false; }
                    // - - CALCIO LIGNOSULFONATO SOLIDO -> id MMPP 7
                    valorCargaLigno = await BBDD.ExtraerValorMMPP(ID_Receta, i, 7);
                    if (valorCargaLigno != null) { ProcesoActivo_Ligno = true;} else { valorCargaLigno = 0; ProcesoActivo_Ligno = false; }
                    // - - POTASA -> id MMPP 8
                    valorCargaPotasa = await BBDD.ExtraerValorMMPP(ID_Receta, i, 8);
                    if (valorCargaPotasa != null) { ProcesoActivo_Potasa = true; } else { valorCargaPotasa = 0; ProcesoActivo_Potasa = false;}

                    /*
                    Console.WriteLine($"Valor Carga Solidos 1: {valorCargaSolidos1} | Proceso Activo: {ProcesoActivo_CS1}");
                    Console.WriteLine($"Valor Carga Solidos 2: {valorCargaSolidos2} | Proceso Activo: {ProcesoActivo_CS2}");
                    Console.WriteLine($"Valor Carga Solidos 3: {valorCargaSolidos3} | Proceso Activo: {ProcesoActivo_CS3}");
                    Console.WriteLine($"Valor Carga Agua: {valorCargaAgua} | Proceso Activo: {ProcesoActivo_Agua}");
                    Console.WriteLine($"Valor Carga Agua Recueprada: {valorCargaAguaRecu} | Proceso Activo: {ProcesoActivo_AguaRecup}");
                    Console.WriteLine($"Valor Carga HL Pruebas: {valorCargaAntiEs} | Proceso Activo: {ProcesoActivo_AntiEs}");
                    Console.WriteLine($"Valor Carga Ligno: {valorCargaLigno} | Proceso Activo: {ProcesoActivo_Ligno}");
                    Console.WriteLine($"Valor Carga Potasa: {valorCargaPotasa} | Proceso Activo: {ProcesoActivo_Potasa}");
                    */

                    // - - TIEMPO
                    // EXTRAEMOS AHORA EL VALOR DEL TIEMPO
                    decimal? valorTiempo = 0;
                    bool ProcesoActivo_Tiempo = false;

                    valorTiempo = await BBDD.ExtraerValorTIEMPO(ID_Receta, i);
                    if (valorTiempo != null) { ProcesoActivo_Tiempo = true; } else { valorTiempo = 0; ProcesoActivo_Tiempo = false; }

                    //Console.WriteLine($"Valor Tiempo: {valorTiempo} | Proceso Activo: {ProcesoActivo_Tiempo}");

                    // - - OPERARIO
                    decimal? ExisteOIperario = 0;
                    bool ProcesoActivo_Operario = false;

                    ExisteOIperario = await BBDD.ExtraerOperario(ID_Receta, i);
                    if (ExisteOIperario != null) { ProcesoActivo_Operario = true; } else { ExisteOIperario = 0; ProcesoActivo_Operario = false; }

                    //Console.WriteLine($"Existe Operario: {ExisteOIperario} | Proceso Activo: {ProcesoActivo_Operario}");

                    // - - AGITACION
                    decimal? ExisteAgitacionModo = 0;
                    decimal? ExisteAgitacionVelocidad = 0;
                    decimal? ExisteAgitacionToff = 0;
                    decimal? ExisteAgitacionTon = 0;

                    bool ProcesoActivo_AGModo = false;
                    bool ProcesoActivo_AGVel = false;
                    bool ProcesoActivo_AGToff = false;
                    bool ProcesoActivo_AGTon = false;
                    bool ProcesoActivo_Intermitencia = false;

                    ExisteAgitacionModo = await BBDD.ExtraerAgitacion(ID_Receta, i,"Modo");
                    if (ExisteAgitacionModo != null) { ProcesoActivo_AGModo = true; } else { ExisteAgitacionModo = 0; ProcesoActivo_AGModo = false; }

                    if (ExisteAgitacionModo == 1) { ProcesoActivo_Intermitencia = false;} else {ProcesoActivo_Intermitencia = true;}

                    ExisteAgitacionVelocidad = await BBDD.ExtraerAgitacion(ID_Receta, i, "Velocidad");
                    if (ExisteAgitacionVelocidad != null) { ProcesoActivo_AGVel = true; } else { ExisteAgitacionVelocidad = 0; ProcesoActivo_AGVel = false; }

                    ExisteAgitacionToff = await BBDD.ExtraerAgitacion(ID_Receta, i, "Tiempo OFF");
                    if (ExisteAgitacionToff != null) { ProcesoActivo_AGToff = true; } else { ExisteAgitacionToff = 0; ProcesoActivo_AGToff = false; }

                    ExisteAgitacionTon = await BBDD.ExtraerAgitacion(ID_Receta, i, "Tiempo ON");
                    if (ExisteAgitacionTon != null) { ProcesoActivo_AGTon = true; } else { ExisteAgitacionTon = 0; ProcesoActivo_AGTon = false; }

                    // - - TEMPERATURA
                    decimal? ExisteTempModo = 0;
                    decimal? ExisteTemp = 0;

                    bool ProcesoActivo_TempModo= false;
                    bool ProcesoActivo_Temp = false;

                    ExisteTempModo = await BBDD.ExtraerTemperatura(ID_Receta, i, "Modo");
                    if (ExisteTempModo != null) { ProcesoActivo_TempModo = true; } else { ExisteTempModo = 0; ProcesoActivo_TempModo = false; }

                    ExisteTemp = await BBDD.ExtraerTemperatura(ID_Receta, i, "Temperatura");
                    if (ExisteTemp != null) { ProcesoActivo_Temp = true; } else { ExisteTemp = 0; ProcesoActivo_Temp = false; }

                    // - - EXTAEMOS NOMBRE DE ETAPA

                    string? Nombre_Etapa = await BBDD.ExtraerNombreEtapa(ID_Receta,i);

                    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
                    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
                    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

                    // Cabecera de la etapa
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["n_Etapa"] = i,
                        ["nombre"] = Nombre_Etapa,
                        ["etapaActiva"] = 1
                    }
                });

                    // Procesos de ejemplo (esto debería venir de la base de datos)
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 1,
                        ["tipo"] = "Carga_Solidos_1",
                        ["consigna"] = "Cantidad",
                        ["valor"] = valorCargaSolidos1,
                        ["procesoActivo"] = ProcesoActivo_CS1
                    },
                    new JObject
                    {
                        ["id"] = 1,
                        ["tipo"] = "Carga_Solidos_1",
                        ["consigna"] = "Velocidad_Vibracion",
                        ["valor"] = 2.0
                    },
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 2,
                        ["tipo"] = "Carga_Solidos_2",
                        ["consigna"] = "Cantidad",
                        ["valor"] = valorCargaSolidos2,
                        ["procesoActivo"] = ProcesoActivo_CS2
                    },
                    new JObject
                    {
                        ["id"] = 2,
                        ["tipo"] = "Carga_Solidos_2",
                        ["consigna"] = "Velocidad_Vibracion",
                        ["valor"] = 4.0
                    },
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 3,
                        ["tipo"] = "Carga_Solidos_3",
                        ["consigna"] = "Cantidad",
                        ["valor"] = valorCargaSolidos3,
                        ["procesoActivo"] = ProcesoActivo_CS3
                    },
                    new JObject
                    {
                        ["id"] = 3,
                        ["tipo"] = "Carga_Solidos_3",
                        ["consigna"] = "Velocidad_Vibracion",
                        ["valor"] = 6.0
                    },
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 4,
                        ["tipo"] = "Carga_Agua_Descal",
                        ["consigna"] = "Cantidad",
                        ["valor"] = valorCargaAgua,
                        ["procesoActivo"] = ProcesoActivo_Agua
                    }
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 5,
                        ["tipo"] = "Carga_Agua_Recup",
                        ["consigna"] = "Cantidad",
                        ["valor"] = valorCargaAguaRecu,
                        ["procesoActivo"] = ProcesoActivo_AguaRecup
                    }
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 6,
                        ["tipo"] = "Carga_Antiespumante",
                        ["consigna"] = "Cantidad",
                        ["valor"] = valorCargaAntiEs,
                        ["procesoActivo"] = ProcesoActivo_AntiEs
                    }
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 7,
                        ["tipo"] = "Carga_Ligno",
                        ["consigna"] = "Cantidad",
                        ["valor"] = valorCargaLigno,
                        ["procesoActivo"] = ProcesoActivo_Ligno
                    }
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 8,
                        ["tipo"] = "Carga_Potasa",
                        ["consigna"] = "Cantidad",
                        ["valor"] = valorCargaPotasa ,
                        ["procesoActivo"] = ProcesoActivo_Potasa
                    }
                });
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Espera",
                        ["consigna"] = "Tiempo",
                        ["valor"] = valorTiempo,
                        ["procesoActivo"] = ProcesoActivo_Tiempo
                    }
                });
                    // nuevo se añade operario
                    etapa.Add(new JArray
                {
                    new JObject
                    {
                        ["id"] = 0,
                        ["tipo"] = "Operador",
                        ["consigna"] = "Operador",
                        ["valor"] = 0,
                        ["procesoActivo"] = ProcesoActivo_Operario
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
                        ["valor"] = ExisteAgitacionModo ,
                        ["procesoActivo"] = ProcesoActivo_AGModo
                },
                    new JObject
                {
                        ["id"] = 0,
                        ["tipo"] = "Agitacion",
                        ["consigna"] = "Intermitencia",
                        ["valor"] = ProcesoActivo_Intermitencia
                },
                    new JObject
                {
                        ["id"] = 0,
                        ["tipo"] = "Agitacion",
                        ["consigna"] = "Velocidad",
                        ["valor"] = ExisteAgitacionVelocidad
                }, 
                    new JObject
                {
                        // Valor -> 16
                        ["id"] = 0,
                        ["tipo"] = "Agitacion",
                        ["consigna"] = "Temporizado",
                        ["valor"] = 0
                },
                    new JObject
                {
                        ["id"] = 0,
                        ["tipo"] = "Agitacion",
                        ["consigna"] = "Tiempo_ON",
                        ["valor"] = ExisteAgitacionToff
                },
                    new JObject
                {
                        ["id"] = 0,
                        ["tipo"] = "Agitacion",
                        ["consigna"] = "Tiempo_OFF",
                        ["valor"] = ExisteAgitacionTon
                }    
                }); 
                    etapa.Add(new JArray // Temperatura
                {
                    new JObject
                {
                        ["id"] = 0,
                        ["tipo"] = "Temperatura",
                        ["consigna"] = "Temperatura",
                        ["valor"] = ExisteTemp ,
                        ["procesoActivo"] = ProcesoActivo_Temp
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
                //Console.WriteLine($"Numero de consignas del proceso {Proceso}: {NumeroConsignas}");
                IDsConsignas = new int[NumeroConsignas];

                for (int Etapa = 1; Etapa <= NumeroEtapas; Etapa++) 
                {
                    JArray proceso = (JArray)jsonArray[Etapa][Proceso];
                    int NumeroConsignasProceso = proceso.Count;

                    for (int NumeroConsigna = 0; NumeroConsigna < NumeroConsignasProceso; NumeroConsigna++)
                    {
                        JObject consigna = (JObject)jsonArray[Etapa][Proceso][NumeroConsigna];
                        IDsConsignas[Indice] = consigna.Value<int>("id");
                        //Console.WriteLine($"Etapa: {Etapa}  - ID Consigna: {IDsConsignas[Indice]}");
                        Indice++;
                    }
                }
                Listado.Add(IDsConsignas);
            }

            return Listado;

        }
    }
}
