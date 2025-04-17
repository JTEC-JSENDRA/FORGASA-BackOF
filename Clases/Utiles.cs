using System;
using System.Collections.Generic;
using System.Data;

namespace GestionRecetas.Clases
{
    public class Utiles
    {

        public List<int[]> CompareListArrayINT(int LongListado, List<int[]> Listado1, List<int[]> Listado2)
        {
            List<int[]> ListDiferentes = new List<int[]>();

            for (int i = 0; i < LongListado; i++)
            {
                int[] Array1 = Listado1[i];
                int[] Array2 = Listado2[i];

                // Convertir los arrays de ID en conjuntos HashSet para facilitar la comparación
                HashSet<int> hashSet_Array1 = new HashSet<int>(Array1);
                HashSet<int> hashSet_Array2 = new HashSet<int>(Array2);

                // Obtener los ID que existen en la BBDD pero no en la receta
                HashSet<int> HasSet_Diferentes = new HashSet<int>(hashSet_Array1.Except(hashSet_Array2));

                if (HasSet_Diferentes.Count == 0)
                {
                    int[] Diferentes = new int[1];
                    Diferentes[0] = 0;
                    ListDiferentes.Add(Diferentes);
                }
                else
                {
                    int[] Diferentes = HasSet_Diferentes.ToArray();
                    ListDiferentes.Add(Diferentes);

                }
            }
            return ListDiferentes;
        }
    }
}
