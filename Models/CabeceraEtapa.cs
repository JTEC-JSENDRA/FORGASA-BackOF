using System;
using System.Collections.Generic;

namespace GestionRecetas.Models
{
    public class CabeceraEtapa
    {
        public int ID { get; set; }

        public int N_Etapa { get; set; }

        public string? Nombre { get; set; }

        public short EtapaActiva { get; set; }

    }
}
