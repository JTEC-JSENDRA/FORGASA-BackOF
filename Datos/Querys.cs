using System;
using System.Collections.Generic;
using GestionRecetas.Models;



namespace GestionRecetas.Datos
{
    public class Querys
    {

        public string Select(int Consulta, string Valor = "", short Etapa = 0, string Proceso = "")
        {
           List<string> querys = new List<string>();

            //0 - Obtener cabecera de la receta
            querys.Add($@"SELECT
                            R.ID,
                            R.Nombre AS NombreReceta,
                            T2.Nombre AS NombreReactor,
                            R.NumeroEtapas,
                            R.Creada,
                            R.Modificada,
                            R.Eliminada
                        FROM Recetas R
                        JOIN Reactores T2 ON R.ID_Reactor = T2.ID
                        WHERE R.Nombre LIKE '{Valor}' AND T2.ID = R.ID_Reactor;");

            //1 - Obtener cabecera de la etapa
            querys.Add($@"SELECT ID, N_Etapa, Nombre, EtapaActiva
                        FROM Etapas
                        WHERE N_Etapa = {Etapa}
                        AND ID_Receta IN(SELECT ID FROM Recetas WHERE Nombre LIKE '{Valor}')");

            //2 - Obtener datos del proceso seleccionado
            querys.Add($@"SELECT ID, Tipo, Consigna, Valor
                        From {Proceso} --Viene definido por listado obtenido
                        WHERE N_Etapa = {Etapa}
                        AND ID_Receta IN (SELECT ID FROM Recetas WHERE Nombre LIKE '{Valor}')");

            //3 - Obtener datos del proceso secundario
            querys.Add($@"SELECT ID, Nombre FROM Procesos ORDER BY ID");

            //4 - Comprobar si la receta existe en la BBDD 
            querys.Add($@"SELECT COUNT(*) FROM Recetas WHERE ID = '{Valor}'");

            //5 - Comprobar si la etapa existe en la BBDD 
            querys.Add($@"SELECT COUNT(*) FROM Etapas WHERE ID = '{Valor}'");

            //6 - Comprobar si la consigna existe en la BBDD 
            querys.Add($@"SELECT COUNT(*) FROM Proceso{Proceso} WHERE ID = '{Valor}'");

            //7 - Se obtienen todos los IDs de las etapas de la receta seleccionadas
            querys.Add($@"SELECT ID FROM Etapas WHERE ID_Receta = '{Valor}'");

            //8 - Se obtienen todos los IDs de las consignas de las tablas de proceso de la receta seleccionadas
            querys.Add($@"SELECT ID FROM Proceso{Proceso} WHERE ID_Receta = '{Valor}'");

            //9 - Contador de elementos de una tabla condicionado
            querys.Add($@"SELECT COUNT(*) FROM {Proceso} WHERE ID_Receta = '{Valor}'");

            //10 - Contador de elementos de una tabla
            querys.Add($@"SELECT COUNT(*) FROM {Valor} ");

            return querys[Consulta];
        }

        public string Insert(int Consulta, CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 Consigna, string Proceso = "")
        {
            List<string> querys = new List<string>();

            //0 - Se inserta la receta en caso de que no exista
            querys.Add($@"INSERT INTO Recetas (ID_Reactor, Nombre, Bloqueada, NumeroEtapas, Creada) 
                            VALUES ((SELECT ID FROM Reactores WHERE Nombre LIKE '{CabeceraReceta.NombreReactor}'), '{CabeceraReceta.NombreReceta}', {0}, {CabeceraReceta.NumeroEtapas}, GETDATE())");

            //1 - Se inserta la cabecera de la etapa en caso de que no exista 
            querys.Add($@"INSERT INTO Etapas (ID_Receta, EtapaActiva, N_Etapa, Nombre) 
                            VALUES ({CabeceraReceta.ID}, {CabeceraEtapa.EtapaActiva}, {CabeceraEtapa.N_Etapa}, '{CabeceraEtapa.Nombre}')");

            //2 - Se inserta la consigna en caso de que no exista en la BBDD 
            querys.Add($@"INSERT INTO Proceso{Proceso} (ID_Receta, N_Etapa, Tipo, Consigna, Valor) 
                            VALUES ({CabeceraReceta.ID}, {CabeceraEtapa.N_Etapa}, '{Consigna.Tipo}', '{Consigna.Consigna}', {Consigna.Valor})");

            return querys[Consulta];
        }

        public string Update(int Consulta, CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 Consigna, string Proceso = "")
        {
            List<string> querys = new List<string>();

            //0 - Se actualizan los valores de la receta 
            querys.Add($@"UPDATE Recetas 
                            SET ID_Reactor = (SELECT ID FROM Reactores WHERE Nombre LIKE '{CabeceraReceta.NombreReactor}'),
                                Nombre = '{CabeceraReceta.NombreReceta}',
                                Bloqueada = {0},
                                NumeroEtapas = {CabeceraReceta.NumeroEtapas},
                                Modificada = GETDATE()
                            WHERE ID LIKE '{CabeceraReceta.ID}'");

            //1 - Se actualizan los valores de la cabecera
            querys.Add($@"UPDATE Etapas 
                            SET ID_Receta =  {CabeceraReceta.ID},
                                EtapaActiva = {CabeceraEtapa.EtapaActiva},
                                N_Etapa = {CabeceraEtapa.N_Etapa},
                                Nombre = '{CabeceraEtapa.Nombre}'
                            WHERE ID LIKE {CabeceraEtapa.ID}");

            //2 - Se actualizan los valores de la consigna
            querys.Add($@"UPDATE Proceso{Proceso} 
                            SET ID_Receta =  {CabeceraReceta.ID},
                                N_Etapa = {CabeceraEtapa.N_Etapa},
                                Tipo = '{Consigna.Tipo}',
                                Consigna = '{Consigna.Consigna}',
                                Valor = {Consigna.Valor}
                            WHERE ID LIKE {Consigna.ID}");

            return querys[Consulta];
        }

        public string Delete(int Consulta, string Tabla, int ID)
        {
            List<string> querys = new List<string>();
            
            //0 - Se eliminan todas las entradas que ya no existen en la receta
            querys.Add($@"DELETE FROM {Tabla} WHERE ID = {ID}");

            return querys[Consulta];
        }

    }
}

