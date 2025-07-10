using System;
using System.Collections.Generic;
using System.Data;

namespace GestionRecetas.Clases
{
    public class Utiles
    {
        // ---------------------------------------------------------------------------------------------------------------------------

        // Este método compara dos listas de arrays de enteros (List<int[]>), 
        // elemento por elemento, y devuelve una lista con los IDs que están en Listado1 pero no en Listado2.
        // Si no hay diferencias, añade un array con un solo 0 para indicar que no hay diferencias.

        public List<int[]> CompareListArrayINT(int LongListado, List<int[]> Listado1, List<int[]> Listado2)
        {
            // Lista donde guardaremos los arrays con los IDs diferentes para cada posición
            List<int[]> ListDiferentes = new List<int[]>();

            // Recorremos cada posición de las listas que queremos comparar
            for (int i = 0; i < LongListado; i++)
            {
                int[] Array1 = Listado1[i];             // Array en la posición i de Listado1
                int[] Array2 = Listado2[i];             // Array en la posición i de Listado2

                // Convertimos los arrays en HashSet para facilitar la comparación (conjuntos sin elementos repetidos)
                HashSet<int> hashSet_Array1 = new HashSet<int>(Array1);
                HashSet<int> hashSet_Array2 = new HashSet<int>(Array2);

                // Calculamos los elementos que están en Array1 pero NO en Array2
                HashSet<int> HasSet_Diferentes = new HashSet<int>(hashSet_Array1.Except(hashSet_Array2));

                // Si no hay diferencias
                if (HasSet_Diferentes.Count == 0)
                {
                    int[] Diferentes = new int[1];      // Creamos un array de tamaño 1
                    Diferentes[0] = 0;                  // Con un 0 para indicar "no hay diferencias"
                    ListDiferentes.Add(Diferentes);     // Lo añadimos a la lista de resultados
                }
                else
                {
                    // Si sí hay diferencias, las convertimos a array y las añadimos a la lista
                    int[] Diferentes = HasSet_Diferentes.ToArray();
                    ListDiferentes.Add(Diferentes);

                }
            }
            // Devolvemos la lista con arrays de diferencias por cada posición comparada
            return ListDiferentes;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

    }
}
