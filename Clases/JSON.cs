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
        // ---------------------------------------------------------------------------------------------------------------------------

        private readonly JsonElement jsonData;

        // ---------------------------------------------------------------------------------------------------------------------------

        // Método para transformar un array de recetas JSON en un nuevo formato que incluye las etapas y procesos de cada receta.
        // También consulta la base de datos para obtener valores detallados de cada etapa (como cantidades, tiempos, etc).

        public static async Task<JArray> TransformarJson(JArray jsonArray, SQLServerManager BBDD)
        {
            // Creamos un nuevo array JSON donde guardaremos el resultado final
            JArray recetaFinal = new JArray();

            // Recorremos cada receta del array recibido
            foreach (JObject receta in jsonArray)
            {
                // Creamos un array que contiene la información principal de la receta (cabecera)
                JArray recetaCabecera = new JArray
            {
                new JObject
                {
                    ["id"] = 0,
                    ["ordenFabricacion"] = receta["ordenFabricacion"],  // Número de orden de fabricación
                    ["nombreReceta"] = receta["nombreReceta"],          // Nombre de la receta
                    ["version"] = receta["version"],                    // Versión de la receta
                    ["nombreReactor"] = receta["nombreReactor"],        // Nombre del reactor asignado
                    ["numeroEtapas"] = receta["numeroEtapas"],          // Número total de etapas
                    ["creada"] = receta["creada"],                      // Fecha de creación
                    ["modificada"] = receta["modificada"],              // Fecha de última modificación
                    ["eliminada"] = receta["eliminada"]                 // Indica si fue eliminada (true/false)
                }
            };

                // Añadimos la cabecera al resultado final
                recetaFinal.Add(recetaCabecera);

                // Extraemos el nombre de la receta para poder buscar su ID en la base de datos
                string nombreReceta = receta["nombreReceta"]?.ToString();
                int versionReceta = (int)receta["version"];

                int? ID_Receta = await BBDD.ObtenerIDReceta(versionReceta, nombreReceta);


                // Recorremos todas las etapas de la receta, una por una
                for (int i = 1; i <= (int)receta["numeroEtapas"]; i++)
                {
                    // Creamos una nueva etapa en formato JSON
                    JArray etapa = new JArray();

                    // Variables para guardar los valores de materia prima de esta etapa
                    // Por defecto son 0, pero se actualizan si hay datos en la BD
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

                    // Indicadores de si cada proceso está activo o no
                    bool ProcesoActivo_CS1 = false;
                    bool ProcesoActivo_CS2 = false;
                    bool ProcesoActivo_CS3 = false;
                    bool ProcesoActivo_Agua = false;
                    bool ProcesoActivo_AguaRecup = false;
                    bool ProcesoActivo_AntiEs = false;
                    bool ProcesoActivo_Ligno = false;
                    bool ProcesoActivo_Potasa = false;

                    // Consultamos cada tipo de materia prima según el ID de receta, número de etapa y el ID del producto
                    // Si hay un valor en la BD, lo guardamos y marcamos como "activo", si no, dejamos 0 y lo marcamos como "inactivo"

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

                    // - - TIEMPO

                    // Consultamos el tiempo de espera en esta etapa
                    decimal? valorTiempo = 0;
                    bool ProcesoActivo_Tiempo = false;

                    valorTiempo = await BBDD.ExtraerValorTIEMPO(ID_Receta, i);
                    if (valorTiempo != null) { ProcesoActivo_Tiempo = true; } else { valorTiempo = 0; ProcesoActivo_Tiempo = false; }

                    // - - OPERARIO

                    // Comprobamos si se requiere intervención de operario en esta etapa
                    decimal? ExisteOIperario = 0;
                    bool ProcesoActivo_Operario = false;

                    ExisteOIperario = await BBDD.ExtraerOperario(ID_Receta, i);
                    if (ExisteOIperario != null) { ProcesoActivo_Operario = true; } else { ExisteOIperario = 0; ProcesoActivo_Operario = false; }

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

                    // Ahora empezamos a construir la etapa con todos los procesos asociados

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

                    // A continuación agregamos cada proceso con su tipo, consigna, valor y estado (activo o no)
                    // Lo ideal sería automatizar esto, pero por ahora es manual y repetitivo

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

                    // Finalmente añadimos esta etapa a la receta final
                    recetaFinal.Add(etapa);
                }
            }
            // Devolvemos el JSON final con todas las recetas transformadas
            return recetaFinal;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        public JSON(JsonElement jsonData)
        {
            this.jsonData = jsonData;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Este método extrae la información de la cabecera (parte principal) de una receta desde un objeto JSON

        public CabeceraReceta ObtenerCabeceraReceta()
        {
            // Creamos un nuevo objeto de tipo CabeceraReceta donde almacenaremos los datos
            CabeceraReceta Cabecera = new CabeceraReceta();

            // Convertimos el objeto jsonData (que contiene datos en formato JSON) a una cadena de texto
            string jsonString = jsonData.ToString();

            // Analizamos (parseamos) la cadena JSON y la convertimos en un arreglo JArray de Newtonsoft.Json
            JArray jsonArray = JArray.Parse(jsonString);

            // Accedemos al primer objeto dentro del JSON. La cabecera siempre está en la primera posición [0][0].
            JObject primerObjeto = (JObject)jsonArray[0][0];

            // Asignamos los valores del JSON a las propiedades de la cabecera
            Cabecera.ID = primerObjeto.Value<int>("id");                                // ID de la receta
            Cabecera.NombreReceta = primerObjeto.Value<string>("nombreReceta");         // Nombre de la receta
            Cabecera.NombreReactor = primerObjeto.Value<string>("nombreReactor");       // Nombre del reactor usado
            Cabecera.NumeroEtapas = primerObjeto.Value<short>("numeroEtapas");          // Número total de etapas de la receta

            // Verificamos si el campo "creada" no es nulo antes de asignarlo
            if (primerObjeto["modificada"].Type != JTokenType.Null)
            {
                Cabecera.Creada = primerObjeto.Value<DateTime>("creada");               // Fecha de creación de la receta
            }
            // Verificamos si el campo "modificada" no es nulo antes de asignarlo
            if (primerObjeto["modificada"].Type != JTokenType.Null)
            {
                Cabecera.Modificada = primerObjeto.Value<DateTime>("modificada");       // Fecha de la última modificación
            }
            // Verificamos si el campo "eliminada" no es nulo antes de asignarlo
            if (primerObjeto["eliminada"].Type != JTokenType.Null)
            {
                Cabecera.Eliminada = primerObjeto.Value<DateTime>("eliminada");         // Fecha de eliminación, si existe
            }
        
            // Finalmente, retornamos el objeto cabecera con todos los datos obtenidos del JSON
            return Cabecera;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Este método obtiene la cabecera (información principal) de una etapa específica de una receta

        public CabeceraEtapa ObtenerCabeceraEtapa(short NumeroEtapa)
        {
            // Creamos un nuevo objeto de tipo CabeceraEtapa para guardar la información extraída
            CabeceraEtapa Cabecera = new CabeceraEtapa();

            // Convertimos el objeto jsonData (que contiene los datos en formato JSON) a una cadena de texto
            string jsonString = jsonData.ToString();

            // Analizamos (parseamos) la cadena JSON para convertirla a un JArray (array JSON) usando la librería Newtonsoft.Json
            JArray jsonArray = JArray.Parse(jsonString);

            // -----------------------------------------
            // Cómo acceder a los datos:
            // jsonArray[NumeroEtapa] → accede a la etapa N (por ejemplo: etapa 1, etapa 2, etc.)
            // [0] → accede al primer bloque dentro de la etapa (que es la cabecera de esa etapa)
            // [0] → accede al primer objeto dentro de ese bloque (que contiene los datos como id, nombre, etc.)
            // -----------------------------------------

            // Accedemos a la cabecera de la etapa solicitada
            JObject primerObjeto = (JObject)jsonArray[NumeroEtapa][0][0];

            // Extraemos y asignamos los valores del JSON al objeto Cabecera
            Cabecera.ID = primerObjeto.Value<int>("id");                            // ID de la etapa
            Cabecera.EtapaActiva = primerObjeto.Value<short>("etapaActiva");        // Si la etapa está activa (1 = sí, 0 = no)
            Cabecera.N_Etapa = primerObjeto.Value<short>("n_Etapa");                // Número de etapa (1, 2, 3, ...)
            Cabecera.Nombre = primerObjeto.Value<string>("nombre");                 // Nombre descriptivo de la etapa

            // Devolvemos la cabecera ya completa
            return Cabecera;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Este método devuelve una lista de consignas (instrucciones o parámetros)
        // para un proceso específico dentro de una etapa determinada.

        public List<CsgProceso1> ObtenerConsignasProceso(int NumeroEtapa, int NumeroProceso)
        {
            // Creamos una lista vacía donde vamos a guardar las consignas del proceso
            List<CsgProceso1> ListaConsignas = new List<CsgProceso1>();

            // Convertimos el objeto JSON (jsonData) a una cadena de texto
            string jsonString = jsonData.ToString();

            // Parseamos la cadena JSON a un arreglo (JArray), que podemos recorrer como una lista
            JArray jsonArray = JArray.Parse(jsonString);

            // -----------------------------------------
            // Cómo acceder a los datos:
            // jsonArray[NumeroEtapa]         → accede a la etapa deseada
            // [NumeroProceso]                → accede al proceso deseado dentro de esa etapa
            // esto da como resultado otro arreglo con varias consignas
            // -----------------------------------------

            // Obtenemos el array de consignas del proceso seleccionado
            JArray proceso = (JArray)jsonArray[NumeroEtapa][NumeroProceso];

            // Contamos cuántas consignas hay en ese proceso
            int numeroConsignas = proceso.Count;

            // Recorremos todas las consignas usando un bucle for
            for (int NumeroConsigna = 0; NumeroConsigna < numeroConsignas; NumeroConsigna++)
            {
                // Creamos un nuevo objeto para guardar una consigna
                CsgProceso1 Consigna = new CsgProceso1();

                // Obtenemos la consigna específica en la posición actual
                JObject primerObjeto = (JObject)jsonArray[NumeroEtapa][NumeroProceso][NumeroConsigna];

                // Extraemos los datos de la consigna desde el JSON y los asignamos al objeto
                Consigna.ID = primerObjeto.Value<int>("id");                        // ID único de la consigna
                Consigna.Tipo = primerObjeto.Value<string>("tipo");                 // Tipo de consigna (por ejemplo: temperatura, presión, etc.)
                Consigna.Consigna = primerObjeto.Value<string>("consigna");         // Nombre o código de la consigna
                Consigna.Valor = primerObjeto.Value<string>("valor");               // Valor asignado a esa consigna

                // Agregamos la consigna a la lista
                ListaConsignas.Add(Consigna);
            }
            // Devolvemos la lista completa de consignas para ese proceso
            return ListaConsignas;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Este método devuelve una lista de arrays de enteros con los IDs de:
        // - Todas las etapas
        // - Todas las consignas de cada proceso (excepto el proceso 0, que se asume que es cabecera)

        public List<int[]> GetListadosID(int NumeroProcesos, int NumeroEtapas)
        {
            // Lista que contendrá los arrays de IDs: primero los de etapas, luego los de cada proceso
            List<int[]> Listado = new List<int[]>();

            // Convertimos los datos JSON en una cadena y luego los parseamos a un JArray
            string jsonString = jsonData.ToString();
            JArray jsonArray = JArray.Parse(jsonString);

            // Creamos un array para guardar los IDs de cada etapa
            int[] IDsEtapas = new int[NumeroEtapas];

            // Variables auxiliares
            int[] IDsConsignas;
            int NumeroConsignas;
            int Indice;

            // -----------------------------------------------
            // 1. Obtener los IDs de todas las etapas
            // -----------------------------------------------
            for (int Etapa = 0; Etapa < NumeroEtapas; Etapa++)
            {
                // jsonArray[Etapa + 1][0][0] accede a la cabecera de cada etapa
                JObject etapa = (JObject)jsonArray[Etapa + 1][0][0];
                IDsEtapas[Etapa] = etapa.Value<int>("id");              // Extraemos el ID de la etapa
            }
            // Agregamos los IDs de las etapas a la lista principal
            Listado.Add(IDsEtapas);

            // -----------------------------------------------
            // 2. Obtener los IDs de las consignas por proceso
            // Se comienza desde Proceso = 1 porque Proceso 0 se usa para cabecera
            // -----------------------------------------------
            for (int Proceso = 1; Proceso < NumeroProcesos; Proceso++)
            {
                Indice = 0;
                NumeroConsignas = 0;

                // Primero contamos cuántas consignas hay en total para este proceso
                for (int Etapa = 1; Etapa <= NumeroEtapas; Etapa++)
                {
                    JArray proceso = (JArray)jsonArray[Etapa][Proceso];
                    NumeroConsignas = proceso.Count + NumeroConsignas;  // Acumulamos la cantidad de consignas
                }
                // Creamos un array del tamaño exacto que necesitamos para guardar los IDs
                IDsConsignas = new int[NumeroConsignas];

                // Ahora recorremos nuevamente y llenamos el array con los IDs de cada consigna
                for (int Etapa = 1; Etapa <= NumeroEtapas; Etapa++) 
                {
                    JArray proceso = (JArray)jsonArray[Etapa][Proceso];
                    int NumeroConsignasProceso = proceso.Count;

                    for (int NumeroConsigna = 0; NumeroConsigna < NumeroConsignasProceso; NumeroConsigna++)
                    {
                        // Obtenemos el objeto consigna y extraemos su ID
                        JObject consigna = (JObject)jsonArray[Etapa][Proceso][NumeroConsigna];
                        IDsConsignas[Indice] = consigna.Value<int>("id");
                        Indice++;
                    }
                }
                // Agregamos el array de IDs de consignas para este proceso a la lista principal
                Listado.Add(IDsConsignas);
            }
            // Finalmente, devolvemos la lista que contiene:
            // - [0] → IDs de las etapas
            // - [1..n] → IDs de las consignas por proceso
            return Listado;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

    }
}
