using System;
using System.Collections.Generic;

namespace GestionRecetas.Models
{
    public class CabeceraReceta
    {
        public int ID { get; set; }
        public string? NombreReceta { get; set; }
        public string? NombreReactor { get; set; }

        public short NumeroEtapas { get; set; }

        public DateTime? Creada { get; set; }

        public DateTime? Modificada { get; set; }

        public DateTime? Eliminada { get; set; }
    }
}
