using System;
using System.Collections.Generic;

namespace GestionRecetas.Models
{
    public class CsgProceso
    {
        public int ID { get; set; }
        public string? Tipo { get; set; }
        public string? Consigna { get; set; }
        public decimal Valor { get; set; }
    }
}
